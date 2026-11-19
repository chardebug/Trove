// This is what MainViewModel depends on
namespace Trove.Services;

public interface IVideoPlayerLauncher
{
    void Play(string videoPath);
}
