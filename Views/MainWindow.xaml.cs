using MinimalPlayback.Controllers;
using MinimalPlayback.Helpers;
using MinimalPlayback.Services;
using MinimalPlayback.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MinimalPlayback.Views
{
    public partial class MainWindow : Window
    {
        private List<string> playlist = new List<string>();
        private int currentIndex = -1;

        private readonly VlcPlayerService _player;
        private readonly TimelineState _timeline;

        public MainWindow()
        {
            InitializeComponent();

            _player = new VlcPlayerService();
            _timeline = new TimelineState();

            videoView.MediaPlayer = _player.MediaPlayer;

            _player.TimeChanged += OnTimeChanged;
            _player.LengthChanged += OnLengthChanged;
            _player.EndReached += OnEndReached;
        }

        // ===== СОБЫТИЯ ПЛЕЕРА =====
        private void OnTimeChanged(long time) 
        {
            Dispatcher.Invoke(() =>
            {
                if (Math.Abs(Slider.Value - time) > 500) // Чтобы слайдер не обновлялся каждую мс, а раз в 500мс.
                    Slider.Value = time;

                CurrentTimeText.Text = TimeFormatter.Format(time);
            });
        }

        private void OnLengthChanged(long length)
        {
            Dispatcher.Invoke(() =>
            {
                Slider.Maximum = length;
                TotalTimeText.Text = TimeFormatter.Format(length);
            });
        }

        private void OnEndReached()
        {
            Dispatcher.Invoke(() =>
            {
                PlayNextVideo();
            });
        }

        // ===== СЛАЙДЕР =====
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _timeline.StartDrag();
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _timeline.EndDrag();
            _player.SetTime((long)Slider.Value);
        }

        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Slider).X;

            var time = TimelineMath.CalculateTime(
                pos,
                Slider.ActualWidth,
                Slider.Maximum
            );

            Slider.Value = time;
            _player.SetTime(time);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_timeline.IsDragging)
                return;

            CurrentTimeText.Text = TimeFormatter.Format((long)Slider.Value);
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            var preview = TimelineMath.CalculateTime(
            e.GetPosition(Slider).X,
            Slider.ActualWidth,
            Slider.Maximum
             );

            Slider.ToolTip = TimeFormatter.Format(preview);
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e)
        {
            Slider.ToolTip = null;
        }

        // ===== КНОПКИ =====
        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.Content.ToString() == "Pause")
            {
                _player.Pause();
                button.Content = "Play";
            }
            else
            {
                _player.Resume();
                button.Content = "Pause";
            }
        }
        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayNextVideo();
            });
        }
        private void PlayNextVideo()
        {
            if (playlist.Count == 0)
                return;

            currentIndex++;

            if (currentIndex >= playlist.Count)
                currentIndex = 0; // зацикливание

            PlaylistListBox.SelectedIndex = currentIndex;

            _player.PlayVideo(playlist[currentIndex]);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov"
            };

            if (dialog.ShowDialog() == true)
            {
                playlist.Clear();
                playlist.Add(dialog.FileName);

                currentIndex = 0;

                PlaylistListBox.Items.Clear();
                PlaylistListBox.Items.Add(System.IO.Path.GetFileName(dialog.FileName));
                PlaylistListBox.SelectedIndex = 0;

                _player.PlayVideo(dialog.FileName);
            }
        }

        private void PlaylistBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistListBox.Visibility == Visibility.Collapsed)
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog();
                var result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LoadPlaylist(dlg.SelectedPath);
                }
            }
            PlaylistListBox.Visibility = PlaylistListBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void LoadPlaylist(string folder)
        {
            PlaylistListBox.Items.Clear();
            playlist.Clear();

            var files = Directory.GetFiles(folder, "*.*");

            foreach (var f in files)
            {
                if (f.EndsWith(".mp4") || f.EndsWith(".mkv") || f.EndsWith(".avi") || f.EndsWith(".mov"))
                {
                    playlist.Add(f);
                    PlaylistListBox.Items.Add(System.IO.Path.GetFileName(f));
                }
            }

            if (playlist.Count > 0)
            {
                currentIndex = 0;
                PlaylistListBox.SelectedIndex = 0;
                _player.PlayVideo(playlist[currentIndex]);
            }
        }

        private void PlaylistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistListBox.SelectedIndex < 0)
                return;

            currentIndex = PlaylistListBox.SelectedIndex;

            _player.PlayVideo(playlist[currentIndex]);
        }

        protected override void OnClosed(EventArgs e)
        {
            _player.Dispose();
            base.OnClosed(e);
        }

        private void OpenConvertWindow_Click(object sender, RoutedEventArgs e)
        {
            ConvertWindow window = new ConvertWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}