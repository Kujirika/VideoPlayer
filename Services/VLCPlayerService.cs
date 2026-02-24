using LibVLCSharp.Shared;
using System;

namespace MinimalPlayback.Services
{
    public class VlcPlayerService : IDisposable
    {
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;

        public MediaPlayer MediaPlayer => _mediaPlayer;

        public event Action<long>? TimeChanged;
        public event Action<long>? LengthChanged;
        public event Action? EndReached;

        public VlcPlayerService()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.TimeChanged += (s, e) => TimeChanged?.Invoke(e.Time);
            _mediaPlayer.LengthChanged += (s, e) => LengthChanged?.Invoke(e.Length);
            _mediaPlayer.EndReached += (s, e) => EndReached?.Invoke();
        }

        public void Play(string path)
        {
            var media = new Media(_libVLC, path, FromType.FromPath);
            _mediaPlayer.Play(media);
        }

        public void Pause() => _mediaPlayer.Pause();
        public void Resume() => _mediaPlayer.Play();
        public void Stop() => _mediaPlayer.Stop();

        public void SetTime(long ms) => _mediaPlayer.Time = ms;

        public void Dispose()
        {
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }
    }
}