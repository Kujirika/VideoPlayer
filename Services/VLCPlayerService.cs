using LibVLCSharp.Shared;
using System;

namespace MinimalPlayback.Services
{
    public class VlcPlayerService : IDisposable
    {
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;

        public bool IsPlaying => _mediaPlayer.IsPlaying;
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

        public void PlayVideo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) //Проверка на пустой путь.
                return;

            _mediaPlayer.Stop(); // Иногда VLC держит старое состояние. Остановка старого видео перед новым, чтобы память не ело.

            using (var media = new Media(_libVLC, path)) // using автоматически освобождает память
            {
                _mediaPlayer.Play(media);
            }
        }

        public void Pause()
        {
            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Pause();
        }
        public void Resume()
        {
            if (!_mediaPlayer.IsPlaying)
                _mediaPlayer.Play();
        }
        public void Stop() => _mediaPlayer.Stop();

        public void SetTime(long ms) => _mediaPlayer.Time = ms;

        public void Dispose()
        {
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }
    }
}