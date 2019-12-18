using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Youtube_Downloader
{
	public static class YoutubeHandler
	{
		private static string _id;

		private static readonly YoutubeClient    Client    = new YoutubeClient();
		private static readonly YoutubeConverter Converter = new YoutubeConverter(Client);

		public static bool Parse(string link)
		{
			try
			{
				_id = YoutubeClient.ParseVideoId(link);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		public static async Task<string> GetThumbnailUrlAsync()
		{
			Video video = await Client.GetVideoAsync(_id);

			return video.Thumbnails.MediumResUrl;
		}

		public static async Task DownloadVideoAsync(string            downloadPath,
													object            completedLabel,
													IProgress<double> progress = null)
		{
			await Download("mp4", downloadPath, "Video", progress);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadAudioAsync(string            downloadPath,
													object            completedLabel,
													IProgress<double> progress = null)
		{
			await Download("mp3", downloadPath, "Audio", progress);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadMuteVideoAsync(string            downloadPath,
														object            completedLabel,
														IProgress<double> progress = null)
		{
			MediaStreamInfoSet streamInfoSet = await Client.GetVideoMediaStreamInfosAsync(_id);

			VideoStreamInfo streamInfo = streamInfoSet.Video
													  .Where(s => s.Container == Container.Mp4)
													  .OrderByDescending(s => s.VideoQuality)
													  .ThenByDescending(s => s.Framerate)
													  .First();

			await Download(streamInfo, downloadPath, VideoType.MuteVideo, progress);
			MainWindow.OnComplete(completedLabel);
		}

		private static async Task Download(MediaStreamInfo   streamInfo,
										   string            downloadPath,
										   VideoType         type,
										   IProgress<double> progress = null)
		{
			var ext  = streamInfo.Container.GetFileExtension();
			var path = Path.Combine(downloadPath, $"{_id}_{type}.{ext}");

			await Client.DownloadMediaStreamAsync(streamInfo, path, progress);
		}

		private static async Task Download(string            extenstion,
										   string            downloadPath,
										   string            type     = "Video",
										   IProgress<double> progress = null) =>
			await Converter.DownloadVideoAsync(_id, Path.Combine(downloadPath, $"{_id}_{type}.{extenstion}"), progress);

		private enum VideoType { MuteVideo }
	}
}
