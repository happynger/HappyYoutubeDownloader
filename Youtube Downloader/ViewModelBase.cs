using System.ComponentModel;
using System.Runtime.CompilerServices;
using Youtube_Downloader.Annotations;

namespace Youtube_Downloader
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
