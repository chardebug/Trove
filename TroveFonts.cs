using System;
using Avalonia.Media.Fonts;

public sealed class TroveFonts : EmbeddedFontCollection
{
    public TroveFonts() : base(
        new Uri("fonts:MyFonts", UriKind.Absolute),
        new Uri("avares://Trove/Assets/Fonts", UriKind.Absolute))
    {
    }
}