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

        var resourceName =
            $"{surahId:D3}{verseId:D3}.mp3";

        _currentStream = GetAudioStream(
            resourceName,
            reciterName);

        _currentMedia = new Media(
            _libVLC,
            new StreamMediaInput(_currentStream));

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

    private static Stream GetAudioStream(
        string resourceName,
        string reciterName)
    {
        var assemblyName =
            typeof(AudioHelper)
                .Assembly
                .GetName()
                .Name;

        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            throw new InvalidOperationException(
                "Could not determine assembly name.");
        }

        var uri = new Uri(
            $"avares://{assemblyName}/" +
            $"Data/Audio/{reciterName}/" +
            resourceName);

        return AssetLoader.Open(uri);
    }
}