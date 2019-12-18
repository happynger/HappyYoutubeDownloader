using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Syroot.Windows.IO;
using static System.Windows.Forms.DialogResult;
using TextBox = System.Windows.Controls.TextBox;

namespace Youtube_Downloader
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly Progress<double> _progress;
		private          bool             _canDownload;
		private          string           _folderPath;
		private          bool             _inCorrectId;

		public MainWindow()
		{
			InitializeComponent();
			_progress                 =  new Progress<double>();
			_progress.ProgressChanged += (sender, d) => UpdateProgress(d);
			_folderPath               =  new KnownFolder(KnownFolderType.Downloads).Path;
			TextBlock.Text            =  _folderPath;
		}

		private void BrowseButtonClick(object sender, RoutedEventArgs e)
		{
			var folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() != OK)
				return;

			_folderPath    = folderBrowserDialog.SelectedPath;
			TextBlock.Text = _folderPath;
		}

		public static void OnComplete(object sender)
		{
			if (sender is TextBlock text)
				text.Text = "Completed";
		}

		private void UpdateProgress(double value) => ProgressBar.Value = value;

		private async void UrlBoxChanged(object sender, TextChangedEventArgs e)
		{
			_canDownload      = false;
			Completed.Text    = "";
			ProgressBar.Value = 0;

			if (!(sender is TextBox textBox) || (_inCorrectId = !YoutubeHandler.Parse(textBox.Text)))
				return;

			Preview.Source = new BitmapImage(new Uri(await YoutubeHandler.GetThumbnailUrlAsync()));
			_canDownload   = true;
		}

		private async void DownloadVideoButtonClick(object sender, RoutedEventArgs e)
		{
			if (!_canDownload || !Directory.Exists(_folderPath) || _inCorrectId)
				return;

			await YoutubeHandler.DownloadVideoAsync(_folderPath, Completed, _progress);
		}

		private async void DownloadAudioButtonClick(object sender, RoutedEventArgs e)
		{
			if (!_canDownload || !Directory.Exists(_folderPath) || _inCorrectId)
				return;

			await YoutubeHandler.DownloadAudioAsync(_folderPath, Completed, _progress);
		}

		private async void DownloadSilentVideoButtonClick(object sender, RoutedEventArgs e)
		{
			if (!_canDownload || !Directory.Exists(_folderPath) || _inCorrectId)
				return;

			await YoutubeHandler.DownloadMuteVideoAsync(_folderPath, Completed, _progress);
		}
	}
}
