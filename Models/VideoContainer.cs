namespace Trove.Models;

public class VideoContainer
{
    public long ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
}