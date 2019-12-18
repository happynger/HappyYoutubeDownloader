using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using Syroot.Windows.IO;

namespace Youtube_Downloader
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe")))
				return;

			Console.WriteLine(@"Downloading FFMPEG.exe");
			UpdateFFmpeg();
		}

		private static async void UpdateFFmpeg()
		{
			const string url =
				"https://github.com/vot/ffbinaries-prebuilt/releases/download/v4.1/ffmpeg-4.1-win-64.zip";

			var documentsPath = new KnownFolder(KnownFolderType.Documents).Path;
			var downloadPath  = Path.Combine(documentsPath, "ffmpeg_tmp.zip");

			using (var client = new WebClient())
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				await client.DownloadFileTaskAsync(new Uri(url), downloadPath);
			}

			using (ZipArchive zip = ZipFile.OpenRead(downloadPath))
			{
				zip.GetEntry("ffmpeg.exe")
				   .ExtractToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"));
			}

			File.Delete(downloadPath);
		}
	}
}
