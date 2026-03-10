using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MinimalPlayback.Views
{
    public partial class ConvertWindow : Window
    {
        private string selectedFile;

        public ConvertWindow()
        {
            InitializeComponent();
            CodecCombo.SelectedIndex = 0;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Video files|*.mp4;*.mkv;*.avi;*.mov";

            if (dialog.ShowDialog() == true)
            {
                selectedFile = dialog.FileName;
                FilePathBox.Text = selectedFile;
            }
        }

        private async void StartConvert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Выберите файл");
                return;
            }

            string codec = CodecCombo.SelectedItem.ToString();

            string suffix;

            switch (codec)
            {
                case "libx264":
                    suffix = "_264.mp4";
                    break;

                case "libx265":
                    suffix = "_265.mp4";
                    break;

                default:
                    throw new Exception("Неизвестный кодек");
            }

            string outputFile = Path.Combine(
                Path.GetDirectoryName(selectedFile),
                Path.GetFileNameWithoutExtension(selectedFile) + suffix
            );

            StatusText.Text = "Идёт перекодирование...";

            await Task.Run(() =>
            {
                ConvertVideo(selectedFile, outputFile, codec);
            });

            StatusText.Text = "Готово: " + outputFile;
        }

        private void ConvertVideo(string input, string output, string codec)
        {
            string args = $"-i \"{input}\" -c:v {codec} -crf 23 -preset medium -c:a aac \"{output}\"";

            Process process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
        }
    }
}