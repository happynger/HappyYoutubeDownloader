using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Syroot.Windows.IO;
using YoutubeExplode;
using static System.Windows.Forms.DialogResult;
using TextBox = System.Windows.Controls.TextBox;

namespace Youtube_Downloader
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly Progress<double> progress;
		private          bool             canDownload;
		private          string           folderPath;
		private          bool             isCorrectId;

		public static MainWindow? instance;

		public MainWindow( )
		{
			InitializeComponent();
			progress                 =  new Progress<double>();
			progress.ProgressChanged += ( sender, d ) => UpdateProgress(d);
			folderPath               =  new KnownFolder(KnownFolderType.Downloads).Path;
			TextBlock.Text           =  folderPath;
			instance                 =  this;
		}

		private void BrowseButtonClick( object sender, RoutedEventArgs e )
		{
			var folderBrowserDialog = new FolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() != OK) return;

			folderPath     = folderBrowserDialog.SelectedPath;
			TextBlock.Text = folderPath;
		}

		public static void OnComplete( object sender )
		{
			if (sender is TextBlock text) text.Text = "Completed";
		}

		public static void OnReset( object sender )
		{
			if (sender is TextBlock text) text.Text = "";
		}

		public void ChangePreview( string url )
		{
			Preview.Source = new BitmapImage(new Uri(url));
		}

		private void UpdateProgress( double value ) => ProgressBar.Value = value;

		private async void UrlBoxChanged( object sender, TextChangedEventArgs e )
		{
			canDownload       = false;
			Completed.Text    = "";
			ProgressBar.Value = 0;

			if (!(sender is TextBox textBox) || (isCorrectId = !await YoutubeHandler.ParseAsync(textBox.Text))) return;

			ChangePreview(await YoutubeHandler.GetThumbnailUrlAsync());
			canDownload = true;
		}

		private async void DownloadVideoButtonClick( object sender, RoutedEventArgs e )
		{
			if (!canDownload || !Directory.Exists(folderPath) || isCorrectId) return;

			await YoutubeHandler.DownloadVideoAsync(folderPath, Completed, progress);
		}

		private async void DownloadAudioButtonClick( object sender, RoutedEventArgs e )
		{
			if (!canDownload || !Directory.Exists(folderPath) || isCorrectId) return;

			await YoutubeHandler.DownloadAudioAsync(folderPath, Completed, progress);
		}

		private async void DownloadSilentVideoButtonClick( object sender, RoutedEventArgs e )
		{
			if (!canDownload || !Directory.Exists(folderPath) || isCorrectId) return;

			await YoutubeHandler.DownloadMuteVideoAsync(folderPath, Completed, progress);
		}

		private async void DownloadPlaylistButtonClick( object sender, RoutedEventArgs e )
		{
			if (!canDownload || !Directory.Exists(folderPath) || isCorrectId) return;

			await YoutubeHandler.DownloadPlaylistAsync(folderPath, Completed, progress);
		}
	}
}
