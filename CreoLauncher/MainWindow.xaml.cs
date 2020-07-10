using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Media;
using static CreoLauncher.GamePackage;
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

		private FilesLocal localFiles;

		// Paths
		private string pathRoot;
		private string pathBuild;
		private string pathDownloads;
		private string pathApp;			// Where the application is stored (in /Build)

		// Status
		private LaunchStatus _status;
		internal LaunchStatus Status {
			get => _status;
			set {
				_status = value;

				// Ready State
				if(_status == LaunchStatus.Ready) {
					ButtonPlay.Content = "Play Game";
					StatusLabel.Visibility = Visibility.Hidden;
				}
				
				// Checking for New Updates
				else if(_status == LaunchStatus.CheckingForUpdates) {
					ButtonPlay.Content = "Waiting...";
					this.StatusShow("Checking for Updates..", 255, 235, 35, 40);
				}

				// Updates Available
				else if(_status == LaunchStatus.UpdateAvailable) {
					ButtonPlay.Content = "Update Game";
					StatusLabel.Visibility = Visibility.Hidden;
				}

				// Downloading Update
				else if(_status == LaunchStatus.DownloadingUpdate) {
					ButtonPlay.Content = "Waiting...";
					this.StatusShow("Downloading Updates..", 89, 240, 33, 40);
				}

				// Download Failed
				else if(_status == LaunchStatus.DownloadFailed) {
					ButtonPlay.Content = "Retry Download";
					this.StatusShow("Download Failed!", 240, 33, 33, 40);
				}

				// Installing
				else if(_status == LaunchStatus.Installing) {
					ButtonPlay.Content = "Waiting...";
					this.StatusShow("Installing Updates..", 89, 240, 33, 40);
				}
			}
		}

		public MainWindow() {
			InitializeComponent();

			// Paths
			this.pathRoot = Directory.GetCurrentDirectory();
			this.pathDownloads = Path.Combine(this.pathRoot, Configs.Dir_Download);
			this.pathBuild = Path.Combine(this.pathRoot, Configs.Dir_Build);
			this.pathApp = Path.Combine(this.pathBuild, Configs.Build_Path_Application);

			// Prepare Local Paths
			this.localFiles = new FilesLocal();

			// Create Downloads Directory if it doesn't exist.
			if(!Directory.Exists(this.pathDownloads)) {
				Directory.CreateDirectory(this.pathDownloads);
			}

			// If Local Versioning file doesn't exist, build an empty one.
			string versionPath = Path.Combine(this.pathDownloads, Configs.VersioningFile);
			if(!File.Exists(versionPath)) { File.WriteAllText(versionPath, ""); }
		}

		private void StatusShow(string text, byte red, byte green, byte blue, byte alpha) {
			StatusLabel.Content = text;
			StatusLabel.Background = new SolidColorBrush(Color.FromArgb(red, green, blue, alpha));
			StatusLabel.Visibility = Visibility.Visible;
		}

		private VersioningManager GetLocalVersioningManager() {
			string versionPath = Path.Combine(this.pathDownloads, Configs.VersioningFile);
			string versionStr = File.Exists(versionPath) ? File.ReadAllText(versionPath) : "";
			return new VersioningManager(versionStr);
		}

		private bool SaveVersionString(VersioningManager saveVersionStr) {
			string versionPath = Path.Combine(this.pathDownloads, Configs.VersioningFile);
			File.WriteAllText(versionPath, saveVersionStr.ToString());
			return true;
		}

		private void CheckForUpdates() {
			this.Status = LaunchStatus.CheckingForUpdates;

			// Retrieve the local Versioning and VersioningRules files.
			VersioningManager localVersioning = this.GetLocalVersioningManager();
			bool firstUpdate = !localVersioning.packages.ContainsKey("Game") || localVersioning.packages["Game"].versionID == 0;

			// Update the Visual Label
			// VersionLabel.Content = "Version " + localVersioning.VersionToString();

			// Check For Updates
			try {
				WebClient webClient = new WebClient();

				// Retrieve the Online Versioning file.
				string onlineStr = webClient.DownloadString(Configs.BucketURL + Configs.VersioningFile);
				VersioningManager onlineVersioning = new VersioningManager(onlineStr);

				// Loop through every Online Versioning file:
				foreach(var packages in onlineVersioning.packages) {
					GamePackage onlinePackage = packages.Value;

					// If the Local Versioning doesn't contain this package:
					if(!localVersioning.packages.ContainsKey(packages.Value.title)) {

						// If this package requires an update:
						if(onlinePackage.versionID > 0) {
							this.InstallPackage(onlinePackage);
						}
					}

					else {
						GamePackage localPackage = localVersioning.packages[packages.Value.title];

						if(onlinePackage.versionID < localPackage.versionID) {
							this.InstallPackage(onlinePackage);
						}
					}
				}

				this.Status = LaunchStatus.Ready;
			}
				
			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error while checking for updates: {ex}");
			}
		}

		private void InstallPackage(GamePackage package) {
			try {

				this.Status = LaunchStatus.DownloadingUpdate;
				this.StatusShow("Downloading " + package.title, 89, 240, 33, 40);

				string downloadPath = Path.Combine(this.pathDownloads, package.downloadPath);

				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
				webClient.DownloadFileAsync(new Uri(Configs.BucketURL + package.downloadPath), downloadPath, package);
			}

			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error while downloading game updates: {ex}");
			}
		}

		private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) {
			try {
				GamePackage package = (GamePackage) e.UserState;

				string fromPath = Path.Combine(this.pathDownloads, package.downloadPath);
				string basePath = "";

				// Root Directory Update
				if(package.dirEnum == (byte) DirectoryEnum.RootDirectory) {
					basePath = this.pathRoot;
				}
				
				// Build Path Update
				else if(package.dirEnum == (byte) DirectoryEnum.ContentDirectory) {
					basePath = this.pathBuild;
				}

				// Local AppData Update
				else if(package.dirEnum == (byte) DirectoryEnum.LocalAppData) {
					basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Configs.Dir_LocalApp_Folder);
				}

				// If the basePath wasn't set (shouldn't ever happen), just end here.
				if(basePath.Length == 0) {
					throw new Exception("The basePath was not set correctly due to invalid .dirEnum.");
				}

				string toPath = Path.Combine(basePath, package.finalPath);

				// If we're working with a zip file.
				// Unzip the file into the destination directory and delete the zip afterward.
				if(fromPath.IndexOf(".zip") > 0) {
					ZipFile.ExtractToDirectory(fromPath, toPath, true);
					File.Delete(fromPath);
				}

				// If we're working with a regular file.
				else {
					File.Move(fromPath, toPath);
				}

				// Update the Versioning.txt file with the new values.
				VersioningManager localDetect = this.GetLocalVersioningManager();
				VersioningManager saveDetect = new VersioningManager(onlineVersion.major, onlineVersion.minor, onlineVersion.subMinor, localDetect.planets);
				this.SaveDetectString(saveDetect);

				// Update the Creo Launcher's Label:
				//VersionLabel.Content = "Version " + saveDetect.VersionToString();

				// Update the Status
				this.Status = LaunchStatus.Ready;
				this.StatusShow("Updates Installed!", 33, 163, 37, 255);
			}
			
			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error from installing update: {ex}");
			}
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			this.CheckForUpdates();
		}

		private void Button_PlayGame(object sender, RoutedEventArgs e) {

			// If the game file exists and the launch status is ready, let's play the game.
			if(File.Exists(this.pathApp) && this.Status == LaunchStatus.Ready) {
				ProcessStartInfo startInfo = new ProcessStartInfo(this.pathApp);
				startInfo.WorkingDirectory = Path.Combine(this.pathRoot, "Build");
				Process.Start(startInfo);
				Close();
			}

			else {
				CheckForUpdates();
			}
		}

		private void Button_LaunchWebsite(object sender, RoutedEventArgs e) {
			Process.Start(Configs.WebsiteLaunchURL);
		}
	}

}
