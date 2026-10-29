//This is a function that converts image file paths
//into a bitmap that can be used for rendering images

//this allows the usage of thumbnail uploading

using System.IO;
using System.Linq.Expressions;
using System.Globalization;
using System;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Utilities;

namespace Trove.Converters;

public class PathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string defaultAssetPath = "avares://Trove/Assets/VideoPlaceholderImage.png";
        string? path = value as string;

        //if the path is null, use the default as a fallback
        if(string.IsNullOrWhiteSpace(path))
        {
            path = defaultAssetPath;
        }

        //if the image is in Assets within the project
        if(path.StartsWith("avares://"))
        {
            try
            {
                using var stream = AssetLoader.Open(new Uri(path));
                return Bitmap.DecodeToWidth(stream, 480);
            }
            catch
            {
                //use the fallback
            }
        }

        //if the image is uploaded from another location (like another drive)
        else if(File.Exists(path))
        {
            try
            {
                using var fs = File.OpenRead(path);
                return Bitmap.DecodeToWidth(fs, 480);
            }
            catch
            {
                //if the image fails, use the fallback
            }
        }

        //if all else fails, use the default image
        using var fallback = AssetLoader.Open(new Uri(defaultAssetPath));
        return Bitmap.DecodeToWidth(fallback, 480);

    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}