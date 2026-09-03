using System;
using System.IO;
using LibVLCSharp.Shared;

namespace Quran.Helpers;

public static class AudioHelper
{
    private static readonly LibVLC? LibVlc;
    private static readonly MediaPlayer? MediaPlayer;
    private static Media? _currentMedia;

    static AudioHelper()
    {
        try
        {
            LibVlc = new LibVLC();
            MediaPlayer = new MediaPlayer(LibVlc);
            MediaPlayer.EndReached += (_, _) => { AudioEnded?.Invoke(); };
            IsAvailable = true;
        }
        catch (Exception exception)
        {
            MessageHelper.ShowMessage("Audio Unavailable", exception.Message);
            // Native VLC libraries are missing on this platform.
            // Audio features will be silently disabled.
            IsAvailable = false;
        }
    }

    private static string DataPath => Path.Combine(AppContext.BaseDirectory, "Data");

    /// <summary>True when the native VLC libraries loaded successfully.</summary>
    public static bool IsAvailable { get; }

    public static TimeSpan CurrentPosition =>
        IsAvailable
            ? TimeSpan.FromMilliseconds(Math.Max(0, MediaPlayer!.Time))
            : TimeSpan.Zero;

    public static TimeSpan Duration =>
        IsAvailable
            ? TimeSpan.FromMilliseconds(Math.Max(0, MediaPlayer!.Length))
            : TimeSpan.Zero;

    public static double Position =>
        IsAvailable ? MediaPlayer!.Position * 100 : 0;

    public static bool IsPlaying =>
        IsAvailable && MediaPlayer!.IsPlaying;

    public static int Volume
    {
        get => IsAvailable ? MediaPlayer!.Volume : 0;
        set
        {
            if (IsAvailable)
                MediaPlayer!.Volume = Math.Clamp(value, 0, 100);
        }
    }

    public static event Action? AudioEnded;

    public static void PlayAudio(
        int surahId,
        int verseId,
        string reciterName = "Al-Husary")
    {
        if (!IsAvailable) return;

        StopAudio();

        var fileName = $"{surahId:D3}{verseId:D3}.mp3";
        var audioPath = GetAudioFile(fileName, reciterName);

        _currentMedia = new Media(LibVlc!, audioPath);
        MediaPlayer!.Play(_currentMedia);
    }

    public static void PauseAudio()
    {
        if (IsAvailable && MediaPlayer!.IsPlaying) MediaPlayer.Pause();
    }

    public static void ResumeAudio()
    {
        if (IsAvailable && !MediaPlayer!.IsPlaying) MediaPlayer.Play();
    }

    public static void StopAudio()
    {
        if (!IsAvailable) return;

        MediaPlayer!.Stop();

        // Do not dispose here immediately.
        // LibVLC may still be finishing the previous media.
        _currentMedia = null;
    }

    public static void SeekAudio(double position)
    {
        if (!IsAvailable) return;

        position = Math.Clamp(position, 0, 100);
        MediaPlayer!.Position = (float)(position / 100.0);
    }

    private static string GetAudioFile(string fileName, string reciterName)
    {
        var filePath = Path.Combine(DataPath, "Audio", reciterName, fileName);
        if (File.Exists(filePath)) return filePath;
        throw new FileNotFoundException("Audio file not found.", filePath);
    }
}