namespace Youtube_Downloader
{
	public class MainWindowViewModel : ViewModelBase
	{
		private bool isPlaylist;
		public bool IsPlaylist
		{
			get => isPlaylist;
			set
			{
				isPlaylist = value;
				OnPropertyChanged();
			}
		}

		public static MainWindowViewModel? instance;

		public MainWindowViewModel( ) => instance = this;
	}
}
