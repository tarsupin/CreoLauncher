using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Path = System.IO.Path;

namespace CreoLauncher {

	public enum LaunchStatus : byte {
		Ready,
		CheckingForUpdates,
		UpdateAvailable,
		DownloadingUpdate,
		DownloadFailed,
		Installing,
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		public static MainWindow WindowRef;
		public static LaunchStatus Status;

		public MainWindow() {
			MainWindow.WindowRef = this;
			InitializeComponent();
			Installation.PrepareInstallation();
		}

		public void SetStatus(LaunchStatus status) {
			MainWindow.Status = status;

			// Ready State
			if(MainWindow.Status == LaunchStatus.Ready) {
				ButtonPlay.Content = "Play Game";
				StatusLabel.Visibility = Visibility.Hidden;
			}

			// Checking for New Updates
			else if(MainWindow.Status == LaunchStatus.CheckingForUpdates) {
				ButtonPlay.Content = "Waiting...";
				this.StatusShow("Checking for Updates..", 255, 235, 35, 40);
			}

			// Updates Available
			else if(MainWindow.Status == LaunchStatus.UpdateAvailable) {
				ButtonPlay.Content = "Update Game";
				StatusLabel.Visibility = Visibility.Hidden;
			}

			// Downloading Update
			else if(MainWindow.Status == LaunchStatus.DownloadingUpdate) {
				ButtonPlay.Content = "Waiting...";
				this.StatusShow("Downloading Updates..", 89, 240, 33, 40);
			}

			// Download Failed
			else if(MainWindow.Status == LaunchStatus.DownloadFailed) {
				ButtonPlay.Content = "Retry Download";
				this.StatusShow("Download Failed!", 240, 33, 33, 40);
			}

			// Installing
			else if(MainWindow.Status == LaunchStatus.Installing) {
				ButtonPlay.Content = "Waiting...";
				this.StatusShow("Installing Updates..", 89, 240, 33, 40);
			}
		}

		public void StatusShow(string text, byte red, byte green, byte blue, byte alpha) {
			StatusLabel.Content = text;
			StatusLabel.Background = new SolidColorBrush(Color.FromArgb(red, green, blue, alpha));
			StatusLabel.Visibility = Visibility.Visible;
		}
		
		public void SetVersionLabel(string newVersion) {
			VersionLabel.Content = "Version " + newVersion;
		}
		
		public void SetMessageBox(string message) {
			MessageBox.Show(message);
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			Installation.CheckForUpdates();
		}

		private void Button_PlayGame(object sender, RoutedEventArgs e) {

			// If the game file exists and the launch status is ready, let's play the game.
			if(System.IO.File.Exists(Installation.pathApp) && MainWindow.Status == LaunchStatus.Ready) {
				ProcessStartInfo startInfo = new ProcessStartInfo(Installation.pathApp);
				startInfo.WorkingDirectory = Path.Combine(Installation.pathRoot, "Build");
				Process.Start(startInfo);
				Close();
			}

			else {
				Installation.CheckForUpdates();
			}
		}

		private void Button_LaunchWebsite(object sender, RoutedEventArgs e) {
			string url = Configs.WebsiteLaunchURL;
			try {
				Process.Start(url);
			} catch {
				if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				} else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
					Process.Start("xdg-open", url);
				} else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
					Process.Start("open", url);
				} else {
					throw;
				}
			}
		}
	}

}
