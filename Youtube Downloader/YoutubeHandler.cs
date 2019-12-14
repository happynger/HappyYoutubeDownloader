using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Youtube_Downloader
{
	public static class YoutubeHandler
	{
		private static string _id;

		private static readonly YoutubeClient Client = new YoutubeClient();

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

		public static async Task DownloadVideoAsync(string downloadPath, object completedLabel)
		{
			MediaStreamInfoSet streamInfoSet = await Client.GetVideoMediaStreamInfosAsync(_id);
			MuxedStreamInfo    streamInfo    = streamInfoSet.Muxed.WithHighestVideoQuality();

			await Download(streamInfo, downloadPath, VideoType.Muxed);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadAudioAsync(string downloadPath, object completedLabel)
		{
			MediaStreamInfoSet streamInfoSet = await Client.GetVideoMediaStreamInfosAsync(_id);
			AudioStreamInfo    streamInfo    = streamInfoSet.Audio.WithHighestBitrate();

			await Download(streamInfo, downloadPath, VideoType.Audio);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadMuteVideoAsync(string downloadPath, object completedLabel)
		{
			MediaStreamInfoSet streamInfoSet = await Client.GetVideoMediaStreamInfosAsync(_id);

			VideoStreamInfo streamInfo = streamInfoSet.Video
													  .Where(s => s.Container == Container.Mp4)
													  .OrderByDescending(s => s.VideoQuality)
													  .ThenByDescending(s => s.Framerate)
													  .First();

			await Download(streamInfo, downloadPath, VideoType.MuteVideo);
			MainWindow.OnComplete(completedLabel);
		}

		private static async Task Download(MediaStreamInfo streamInfo, string downloadPath, VideoType type)
		{
			var ext  = streamInfo.Container.GetFileExtension();
			var path = Path.Combine(downloadPath, $"{_id}_{type}.{ext}");

			await Client.DownloadMediaStreamAsync(streamInfo, path);
		}

		private enum VideoType { Audio, MuteVideo, Muxed }
	}
}
