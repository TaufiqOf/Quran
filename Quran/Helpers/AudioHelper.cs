using System;
using System.IO;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace Quran.Helpers;

public static class AudioHelper
{
    private static readonly LibVLC _libVLC;
    private static readonly MediaPlayer _mediaPlayer;

    private static Stream? _currentStream;
    private static Media? _currentMedia;

    public static event Action? AudioEnded;

    static AudioHelper()
    {
        _libVLC = new LibVLC();

        _mediaPlayer = new MediaPlayer(_libVLC);

        _mediaPlayer.EndReached += (_, _) => { AudioEnded?.Invoke(); };
    }

        public static void PlayAudio(
            int surahId,
            int verseId,
            string reciterName = "Al-Husary")
        {
            StopAudio();

            var resourceName = $"{surahId:D3}{verseId:D3}.mp3";
            var audioPath = GetAudioFile(resourceName, reciterName);

            _currentMedia = new Media(
                _libVLC,
                audioPath,
                FromType.FromPath);

            _mediaPlayer.Play(_currentMedia);
        }

        public static void PauseAudio()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
        }

        public static void ResumeAudio()
        {
            if (!_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Play();
            }
        }

        public static void StopAudio()
        {
            _mediaPlayer.Stop();

            // Do not dispose here immediately.
            // LibVLC may still be finishing the previous media.
            _currentMedia = null;

            _currentStream = null;
        }

        public static void SeekAudio(double position)
        {
            position = Math.Clamp(position, 0, 100);

            _mediaPlayer.Position =
                (float)(position / 100.0);
        }

    public static TimeSpan CurrentPosition =>
        TimeSpan.FromMilliseconds(
            Math.Max(0, _mediaPlayer.Time));

    public static TimeSpan Duration =>
        TimeSpan.FromMilliseconds(
            Math.Max(0, _mediaPlayer.Length));

    public static double Position =>
        _mediaPlayer.Position * 100;

    public static bool IsPlaying =>
        _mediaPlayer.IsPlaying;

    public static int Volume
    {
        get => _mediaPlayer.Volume;

        set => _mediaPlayer.Volume =
            Math.Clamp(value, 0, 100);
    }

    private static string GetAudioFile(string resourceName, string reciterName)
    {
        var assemblyName = typeof(AudioHelper).Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new InvalidOperationException("Could not determine assembly name.");
        }

        var resourceUri = new Uri($"avares://{assemblyName}/Data/Audio/{reciterName}/{resourceName}");
        var cacheDirectory = Path.Combine(Path.GetTempPath(), "Quran", "Audio", reciterName);
        Directory.CreateDirectory(cacheDirectory);
        var audioPath = Path.Combine(cacheDirectory, resourceName);
        if (File.Exists(audioPath))
        {
            return audioPath;
        }

        using var sourceStream = AssetLoader.Open(resourceUri);
        using var destinationStream = File.Create(audioPath);
        sourceStream.CopyTo(destinationStream);
        return audioPath;
    }
}