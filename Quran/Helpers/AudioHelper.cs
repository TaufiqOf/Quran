using System;
using System.IO;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace Quran.Helpers;

public static class AudioHelper
{
    private static bool _isInitialized = false;
    private static readonly LibVLC LibVlc;
    private static readonly MediaPlayer MediaPlayer;
    private static Media? _currentMedia;

    static AudioHelper()
    {
        if(_isInitialized) return;
        try
        {
            LibVlc = new LibVLC();
            MediaPlayer = new MediaPlayer(LibVlc);
            MediaPlayer.EndReached += (_, _) => { AudioEnded?.Invoke(); };
        }
        finally{
            _isInitialized = true;
        }
    }

    public static TimeSpan CurrentPosition =>
        TimeSpan.FromMilliseconds(
            Math.Max(0, MediaPlayer.Time));

    public static TimeSpan Duration =>
        TimeSpan.FromMilliseconds(
            Math.Max(0, MediaPlayer.Length));

    public static double Position =>
        MediaPlayer.Position * 100;

    public static bool IsPlaying =>
        MediaPlayer.IsPlaying;

    public static int Volume
    {
        get => MediaPlayer.Volume;

        set => MediaPlayer.Volume =
            Math.Clamp(value, 0, 100);
    }

    public static event Action? AudioEnded;

    public static void PlayAudio(
        int surahId,
        int verseId,
        string reciterName = "Al-Husary")
    {
        StopAudio();

        var resourceName = $"{surahId:D3}{verseId:D3}.mp3";
        var audioPath = GetAudioFile(resourceName, reciterName);

        _currentMedia = new Media(
            LibVlc,
            audioPath);

        MediaPlayer.Play(_currentMedia);
    }

    public static void PauseAudio()
    {
        if (MediaPlayer.IsPlaying) MediaPlayer.Pause();
    }

    public static void ResumeAudio()
    {
        if (!MediaPlayer.IsPlaying) MediaPlayer.Play();
    }

    public static void StopAudio()
    {
        MediaPlayer.Stop();

        // Do not dispose here immediately.
        // LibVLC may still be finishing the previous media.
        _currentMedia = null;
    }

    public static void SeekAudio(double position)
    {
        position = Math.Clamp(position, 0, 100);

        MediaPlayer.Position =
            (float)(position / 100.0);
    }

    private static string GetAudioFile(string resourceName, string reciterName)
    {
        var assemblyName = typeof(AudioHelper).Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new InvalidOperationException("Could not determine assembly name.");

        var resourceUri = new Uri($"avares://{assemblyName}/Data/Audio/{reciterName}/{resourceName}");
        var cacheDirectory = Path.Combine(Path.GetTempPath(), "Quran", "Audio", reciterName);
        Directory.CreateDirectory(cacheDirectory);
        var audioPath = Path.Combine(cacheDirectory, resourceName);
        if (File.Exists(audioPath)) return audioPath;

        using var sourceStream = AssetLoader.Open(resourceUri);
        using var destinationStream = File.Create(audioPath);
        sourceStream.CopyTo(destinationStream);
        return audioPath;
    }
}