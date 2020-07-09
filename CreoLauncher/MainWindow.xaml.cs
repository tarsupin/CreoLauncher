using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Path = System.IO.Path;

namespace CreoLauncher {

	public enum LaunchStatus : byte {
		Ready,
		CheckingForUpdates,
		UpdateAvailable,
		DownloadingGame,
		DownloadingUpdate,
		DownloadingContent,
		DownloadFailed,
		Installing,
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private FilesLocal localFiles;

		// File Paths
		private string pathRoot;
		private string pathBuild;

		// Locations that files are downloaded to:
		private string dlDetectFile;
		private string dlZipFile;
		private string dlPlanetFile;

		// Paths to Files
		private string buildPathGameFile;		// Where the application is stored (in /Build)

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

				// Downloading Game (Initial Download)
				else if(_status == LaunchStatus.DownloadingGame) {
					ButtonPlay.Content = "Waiting...";
					this.StatusShow("Downloading Game..", 89, 240, 33, 40);
				}
				
				// Downloading Update
				else if(_status == LaunchStatus.DownloadingUpdate || _status == LaunchStatus.DownloadingContent) {
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

			this.pathRoot = Directory.GetCurrentDirectory();
			this.pathBuild = Path.Combine(this.pathRoot, Configs.Dir_Build);

			// Download Paths
			this.dlDetectFile = Path.Combine(this.pathRoot, Configs.DL_DetectFile);
			this.dlZipFile = Path.Combine(this.pathRoot, Configs.DL_ZipFile);
			this.dlPlanetFile = Path.Combine(this.pathRoot, Configs.DL_PlanetFile);
			
			// Paths
			this.buildPathGameFile = Path.Combine(this.pathBuild, Configs.Build_Path_Application);

			// Prepare Local Paths
			this.localFiles = new FilesLocal();
		}

		private void StatusShow(string text, byte red, byte green, byte blue, byte alpha) {
			StatusLabel.Content = text;
			StatusLabel.Background = new SolidColorBrush(Color.FromArgb(red, green, blue, alpha));
			StatusLabel.Visibility = Visibility.Visible;
		}

		private DetectData GetLocalDetectData() {
			string detectStr = File.Exists(this.dlDetectFile) ? File.ReadAllText(this.dlDetectFile) : DetectData.emptyStr;
			return new DetectData(detectStr);
		}

		private bool SaveDetectString(DetectData saveDetect) {
			File.WriteAllText(this.dlDetectFile, saveDetect.ToString());
			return true;
		}

		private void CheckForUpdates() {
			this.Status = LaunchStatus.CheckingForUpdates;

			// Retrieve the local Detect.json file. If it doesn't exist, build an empty one.
			DetectData localDetect = this.GetLocalDetectData();
			bool firstUpdate = localDetect.IsEmpty();

			// Update the Visual Label
			VersionLabel.Content = "Version " + localDetect.VersionToString();

			// Check For Updates
			try {
				WebClient webClient = new WebClient();

				// Retrieve the Online Detect file.
				string onlineStr = webClient.DownloadString(Configs.DetectDataURL);
				DetectData onlineDetect = new DetectData(onlineStr);

				if(onlineDetect.HasDifferentVersionThan(localDetect)) {
					this.InstallGameFiles(firstUpdate, onlineDetect);
					return;
				}

				// Check if the Curated Content is Newer:
				if(onlineDetect.planets != localDetect.planets) {
					this.InstallPlanetUpdates(onlineDetect);
					return;
				}

				this.Status = LaunchStatus.Ready;
			}
				
			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error while checking for updates: {ex}");
			}
		}

		private void InstallGameFiles(bool runFullDownload, DetectData _onlineVersion) {
			try {
				this.Status = runFullDownload ? LaunchStatus.DownloadingGame : LaunchStatus.DownloadingUpdate;

				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
				webClient.DownloadFileAsync(new Uri(Configs.GameZipURL), this.dlZipFile, _onlineVersion);
			}

			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error while installing game updates: {ex}");
			}
		}

		private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) {
			try {
				DetectData onlineDetect = (DetectData) e.UserState;

				// Unzip the file into the root directory and delete the zip afterward.
				ZipFile.ExtractToDirectory(this.dlZipFile, this.pathBuild, true);
				File.Delete(this.dlZipFile);

				// Update the Detect.txt file with the new values.
				DetectData localDetect = this.GetLocalDetectData();
				DetectData saveDetect = new DetectData(onlineDetect.major, onlineDetect.minor, onlineDetect.subMinor, localDetect.planets);
				this.SaveDetectString(saveDetect);

				// Update the Creo Launcher's Label:
				VersionLabel.Content = "Version " + saveDetect.VersionToString();

				// Update the Status
				if(this.Status == LaunchStatus.DownloadingGame) {
					this.Status = LaunchStatus.Ready;
					this.StatusShow("Game Installed!", 33, 163, 37, 255);
				} else {
					this.Status = LaunchStatus.Ready;
					this.StatusShow("Updates Installed!", 33, 163, 37, 255);
				}
			}
			
			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error from downloading update: {ex}");
			}
		}

		private void InstallPlanetUpdates(DetectData onlineDetect) {

			// Scan for New Curated Content
			try {
				WebClient webClient = new WebClient();
				this.Status = LaunchStatus.DownloadingUpdate;

				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadPlanetsCompletedCallback);
				webClient.DownloadFileAsync(new Uri(Configs.PlanetContentURL), this.dlPlanetFile, onlineDetect);

			} catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error: issue downloading new content: {ex}");
			}
		}

		private void DownloadPlanetsCompletedCallback(object sender, AsyncCompletedEventArgs e) {
			try {
				DetectData onlineDetect = (DetectData)e.UserState;

				// Save the Planet File into its appropriate local directory:
				string content = File.ReadAllText(this.dlPlanetFile);
				this.localFiles.WriteFile(Configs.Local_Path_Planets, content, true);
				File.Delete(this.dlPlanetFile);

				// Update the Detect.txt file with the new values.
				DetectData localDetect = this.GetLocalDetectData();
				DetectData saveDetect = new DetectData(localDetect.major, localDetect.minor, localDetect.subMinor, onlineDetect.planets);
				this.SaveDetectString(saveDetect);

				// Update the Status
				this.Status = LaunchStatus.Ready;
				this.StatusShow("Updates Installed!", 33, 163, 37, 255);
			}
			
			catch(Exception ex) {
				this.Status = LaunchStatus.DownloadFailed;
				MessageBox.Show($"Error from downloading update: {ex}");
			}
		}

		private void Window_ContentRendered(object sender, EventArgs e) {
			this.CheckForUpdates();
		}

		private void Button_PlayGame(object sender, RoutedEventArgs e) {

			// If the game file exists and the launch status is ready, let's play the game.
			if(File.Exists(this.buildPathGameFile) && this.Status == LaunchStatus.Ready) {
				ProcessStartInfo startInfo = new ProcessStartInfo(this.buildPathGameFile);
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
