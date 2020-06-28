using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Youtube_Downloader
{
	public static class YoutubeHandler
	{
		private static VideoId    id;
		private static PlaylistId playlistId;

		private static readonly Regex IllegalChar = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);

		public static bool IsPlaylist
		{
			get => MainWindowViewModel.instance != null && MainWindowViewModel.instance.IsPlaylist;
			set
			{
				if (MainWindowViewModel.instance == null) return;
				MainWindowViewModel.instance.IsPlaylist = value;
			}
		}

		private static readonly YoutubeClient    Client    = new YoutubeClient();
		private static readonly YoutubeConverter Converter = new YoutubeConverter(Client);

		public static async Task<bool> ParseAsync( string link )
		{
			IsPlaylist = false;
			try
			{
				PlaylistId? tmpId = PlaylistId.TryParse(link);
				IsPlaylist = tmpId != null;
				id         = link;
				playlistId = tmpId ?? throw new ArgumentException();

				return true;
			}
			catch (ArgumentException)
			{
				if (!IsPlaylist || playlistId == null) return false;

				id = (await Client.Playlists.GetVideosAsync(playlistId)).First().Id;
				return true;
			}
		}

		public static async Task<string> GetThumbnailUrlAsync( )
		{
			Video video = await Client.Videos.GetAsync(id);

			return video.Thumbnails.MediumResUrl;
		}

		public static async Task DownloadPlaylistAsync( string downloadPath, object completedLabel, IProgress<double>? progress = null )
		{
			if (!IsPlaylist) return;

			Playlist playlist = await Client.Playlists.GetAsync(playlistId);

			string newPath = Path.Combine(downloadPath, playlist.Title);
			Directory.CreateDirectory(newPath);

			await foreach (Video video in Client.Playlists.GetVideosAsync(playlistId))
			{
				MainWindow.instance?.ChangePreview(video.Thumbnails.MediumResUrl);

				string name = (await Client.Videos.GetAsync(video.Id)).Title;
				name = IllegalChar.Replace(name, replacement: "");
				if (string.IsNullOrEmpty(name)) name = video.Id;

				if (File.Exists(Path.Combine(newPath, $"{name}_Video.mp4"))) continue;

				try
				{
					await DownloadVideoAsync(newPath, video.Id, progress);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}

			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadVideoAsync( string             downloadPath,
													 object             completedLabel,
													 IProgress<double>? progress = null )
		{
			await Download(extenstion: "mp4", downloadPath, VideoType.Video,
						   videoId: null,     progress);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadVideoAsync( string downloadPath, string videoId, IProgress<double>? progress = null )
		{
			await Download(extenstion: "mp4", downloadPath, VideoType.Video,
						   videoId,           progress);
		}

		public static async Task DownloadAudioAsync( string             downloadPath,
													 object             completedLabel,
													 IProgress<double>? progress = null )
		{
			await Download(extenstion: "mp3", downloadPath, VideoType.Audio,
						   videoId: null,     progress);
			MainWindow.OnComplete(completedLabel);
		}

		public static async Task DownloadMuteVideoAsync( string             downloadPath,
														 object             completedLabel,
														 IProgress<double>? progress = null )
		{
			StreamManifest? manifest = await Client.Videos.Streams.GetManifestAsync(id);

			IVideoStreamInfo? streamInfo = manifest.GetVideoOnly().WithHighestVideoQuality();

			if (streamInfo == null) return;

			await Download(streamInfo,    downloadPath, VideoType.MuteVideo,
						   videoId: null, progress);
			MainWindow.OnComplete(completedLabel);
		}

		private static async Task Download( IStreamInfo        streamInfo,
											string             downloadPath,
											VideoType          type,
											string?            videoId  = null,
											IProgress<double>? progress = null )
		{
			string? ext  = streamInfo.Container.Name;
			string  lid  = videoId ?? id;
			string  name = (await Client.Videos.GetAsync(lid)).Title;
			name = IllegalChar.Replace(name, replacement: "");
			if (string.IsNullOrEmpty(name)) name = lid;

			string? path = Path.Combine(downloadPath, $"{name}_{type}.{ext}");

			await Client.Videos.Streams.DownloadAsync(streamInfo, path, progress);
		}

		private static async Task Download( string             extenstion,
											string             downloadPath,
											VideoType          type     = VideoType.Video,
											string?            videoId  = null,
											IProgress<double>? progress = null )
		{
			string lid  = videoId ?? id;
			string name = (await Client.Videos.GetAsync(lid)).Title;
			name = IllegalChar.Replace(name, replacement: "");
			if (string.IsNullOrEmpty(name)) name = lid;
			await Converter.DownloadVideoAsync(lid, Path.Combine(downloadPath, $"{name}_{type}.{extenstion}"), progress);
		}

		private enum VideoType { MuteVideo, Video, Audio }
	}
}
