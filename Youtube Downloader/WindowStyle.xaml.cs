using System.Windows;

namespace Youtube_Downloader
{
	public partial class WindowStyle : ResourceDictionary
	{
		public WindowStyle() { InitializeComponent(); }

		private void CloseClick(object sender, RoutedEventArgs e)
		{
			var window = (Window) ((FrameworkElement) sender).TemplatedParent;
			window.Close();
		}

		private void MaximizeRestoreClick(object sender, RoutedEventArgs e)
		{
			var window = (Window) ((FrameworkElement) sender).TemplatedParent;
			window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
		}

		private void MinimizeClick(object sender, RoutedEventArgs e)
		{
			var window = (Window) ((FrameworkElement) sender).TemplatedParent;
			window.WindowState = WindowState.Minimized;
		}
	}
}
