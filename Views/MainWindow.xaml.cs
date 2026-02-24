using MinimalPlayback.Controllers;
using MinimalPlayback.Helpers;
using MinimalPlayback.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MinimalPlayback.Views;

namespace MinimalPlayback.Views
{
    public partial class MainWindow : Window
    {
        private readonly VlcPlayerService _player;
        private readonly TimelineController _timeline;

        public MainWindow()
        {
            InitializeComponent();

            _player = new VlcPlayerService();
            _timeline = new TimelineController();

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
                if (!_timeline.IsDragging)
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
            Dispatcher.BeginInvoke(() =>
            {
                _player.Stop();
                Slider.Value = 0;
                PlayPauseBtn.Content = "Play";
                CurrentTimeText.Text = "00:00";
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

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_timeline.IsDragging) return;

            var snapped = _timeline.SnapToSeconds(Slider.Value);
            _player.SetTime(snapped);
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            var preview = _timeline.CalculateTime(
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

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov"
            };

            if (dialog.ShowDialog() == true)
                _player.Play(dialog.FileName);
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
            var files = Directory.GetFiles(folder, "*.*");
            foreach (var f in files)
            {
                if (f.EndsWith(".mp4") || f.EndsWith(".mkv") || f.EndsWith(".avi") || f.EndsWith(".mov"))
                    PlaylistListBox.Items.Add(f);
            }
        }

        private void PlaylistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistListBox.SelectedItem != null)
            {
                _player.Play(PlaylistListBox.SelectedItem.ToString());
            }
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