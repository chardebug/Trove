using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Trove.Models;

namespace Trove.Database;

public sealed class VideoRepository
{
    private const string SchemaResourceName =
        "Trove.Database.VideoDatabase.sql";

    private readonly string _connectionString;

    public VideoRepository()
    {
        // Keep the live database in the current user's writable app-data folder.
        var databaseDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Trove");

        Directory.CreateDirectory(databaseDirectory);

        var databasePath = Path.Combine(databaseDirectory, "Trove.db");

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString();

        // Create the tables if they aren't already present.
        using var connection = OpenConnection();

        using var schemaStream =
            typeof(VideoRepository).Assembly.GetManifestResourceStream(
                SchemaResourceName)
            ?? throw new InvalidOperationException(
                $"Could not find embedded schema '{SchemaResourceName}'.");

        using var schemaReader = new StreamReader(schemaStream);
        using var command = connection.CreateCommand();

        command.CommandText = schemaReader.ReadToEnd();
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<VideoContainer> GetAllVideos()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT video_id,
                   COALESCE(title, ''),
                   COALESCE(file_path, ''),
                   COALESCE(thumbnail_path, '')
            FROM Video
            ORDER BY video_id;
            """;

        using var reader = command.ExecuteReader();
        var videos = new List<VideoContainer>();

        while (reader.Read())
        {
            videos.Add(new VideoContainer
            {
                ID = reader.GetInt64(0),
                Title = reader.GetString(1),
                VideoPath = reader.GetString(2),
                ThumbnailPath = reader.GetString(3)
            });
        }

        return videos;
    }

    public long AddVideo(VideoContainer video)
    {
        using var connection = OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Video (title, file_path, thumbnail_path)
            VALUES ($title, $filePath, $thumbnailPath)
            RETURNING video_id;
            """;

        command.Parameters.AddWithValue("$title", video.Title);
        command.Parameters.AddWithValue("$filePath", video.VideoPath);
        command.Parameters.AddWithValue(
            "$thumbnailPath", video.ThumbnailPath);

        return (long)(command.ExecuteScalar()
            ?? throw new InvalidOperationException(
                "SQLite did not return the new video ID."));
    }

    public void DeleteVideos(IReadOnlyCollection<long> videoIds)
    {
        if (videoIds.Count == 0)
        {
            return;
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var deleteTagsCommand = connection.CreateCommand();
        using var deleteVideoCommand = connection.CreateCommand();

        deleteTagsCommand.Transaction = transaction;
        deleteTagsCommand.CommandText =
            "DELETE FROM Video_Tags WHERE video_id = $videoId;";
        var tagsVideoIdParameter =
            deleteTagsCommand.Parameters.Add("$videoId", SqliteType.Integer);

        deleteVideoCommand.Transaction = transaction;
        deleteVideoCommand.CommandText =
            "DELETE FROM Video WHERE video_id = $videoId;";
        var videoIdParameter =
            deleteVideoCommand.Parameters.Add("$videoId", SqliteType.Integer);

        var deletedIds = new HashSet<long>();
        foreach (var videoId in videoIds)
        {
            if (!deletedIds.Add(videoId))
            {
                continue;
            }

            tagsVideoIdParameter.Value = videoId;
            deleteTagsCommand.ExecuteNonQuery();

            videoIdParameter.Value = videoId;
            deleteVideoCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        return connection;
    }
}