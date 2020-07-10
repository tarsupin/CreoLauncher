
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using static CreoLauncher.GamePackage;

namespace CreoLauncher {

	public class Installation {

		// Paths
		public static string pathRoot;
		public static string pathBuild;
		public static string pathContent;
		public static string pathDownloads;
		public static string pathApp;         // Where the application is stored (in /Build)

		public static void PrepareInstallation() {

			// Paths
			Installation.pathRoot = Directory.GetCurrentDirectory();
			Installation.pathDownloads = Path.Combine(Installation.pathRoot, Configs.Dir_Download);
			Installation.pathBuild = Path.Combine(Installation.pathRoot, Configs.Dir_Build);
			Installation.pathContent = Path.Combine(Installation.pathRoot, Configs.Dir_Content);
			Installation.pathApp = Path.Combine(Installation.pathBuild, Configs.Build_Path_Application);

			// Create Downloads Directory if it doesn't exist.
			if(!Directory.Exists(Installation.pathDownloads)) {
				Directory.CreateDirectory(Installation.pathDownloads);
			}

			// If Local Versioning file doesn't exist, build an empty one.
			string versionPath = Path.Combine(Installation.pathDownloads, Configs.VersioningFile);
			if(!File.Exists(versionPath)) { File.WriteAllText(versionPath, ""); }
		}

		private static VersioningData GetLocalVersioningData() {
			string versionPath = Path.Combine(Installation.pathDownloads, Configs.VersioningFile);
			string versionStr = File.Exists(versionPath) ? File.ReadAllText(versionPath) : "";
			return new VersioningData(versionStr);
		}

		private static bool SaveVersionData(VersioningData saveVersion) {
			string versionPath = Path.Combine(Installation.pathDownloads, Configs.VersioningFile);
			File.WriteAllText(versionPath, saveVersion.ToString());
			return true;
		}

		public static void CheckForUpdates() {
			MainWindow.WindowRef.SetStatus(LaunchStatus.CheckingForUpdates);

			// Retrieve the local Versioning and VersioningRules files.
			VersioningData localVersioning = Installation.GetLocalVersioningData();
			bool firstUpdate = !localVersioning.packages.ContainsKey("Game") || localVersioning.packages["Game"].versionID == 0;

			// Update the Visual Label
			// VersionLabel.Content = "Version " + localVersioning.VersionToString();

			// Check For Updates
			try {
				WebClient webClient = new WebClient();

				bool runningUpdates = false;

				// Retrieve the Online Versioning file.
				string onlineStr = webClient.DownloadString(Configs.BucketURL + Configs.VersioningFile);
				VersioningData onlineVersioning = new VersioningData(onlineStr);

				// Loop through every Online Versioning file:
				foreach(var packages in onlineVersioning.packages) {
					GamePackage onlinePackage = packages.Value;

					// If the Local Versioning doesn't contain this package:
					if(!localVersioning.packages.ContainsKey(packages.Value.title)) {

						// If this package requires an update:
						if(onlinePackage.versionID > 0) {
							runningUpdates = true;
							Installation.InstallPackage(onlinePackage);
						}
					} else {
						GamePackage localPackage = localVersioning.packages[packages.Value.title];

						if(onlinePackage.versionID != localPackage.versionID) {
							runningUpdates = true;
							Installation.InstallPackage(onlinePackage);
						}
					}
				}

				// If there are no updates running, indicate the Ready state.
				if(!runningUpdates) {
					MainWindow.WindowRef.SetStatus(LaunchStatus.Ready);
				}
			}
			
			catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error while checking for updates: {ex}");
			}
		}

		private static void InstallPackage(GamePackage package) {
			try {

				// Update Status
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadingUpdate);
				MainWindow.WindowRef.StatusShow("Downloading " + package.title, 89, 240, 33, 40);

				string downloadPath = Path.Combine(Installation.pathDownloads, package.downloadPath);

				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
				webClient.DownloadFileAsync(new Uri(Configs.BucketURL + package.downloadPath), downloadPath, package);
			}
			
			catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error while downloading game updates: {ex}");
			}
		}

		private static void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) {
			try {
				GamePackage package = (GamePackage)e.UserState;

				string fromPath = Path.Combine(Installation.pathDownloads, package.downloadPath);
				string basePath = "";

				// Root Directory (/) Update
				if(package.dirEnum == (byte)DirectoryEnum.RootDirectory) {
					basePath = Installation.pathRoot;
				}

				// Build Path (/Build) Update
				else if(package.dirEnum == (byte)DirectoryEnum.BuildDirectory) {
					basePath = Installation.pathBuild;
				}

				// Content Path (/Build/Content) Update
				else if(package.dirEnum == (byte)DirectoryEnum.ContentDirectory) {
					basePath = Installation.pathContent;
				}

				// Local AppData Update
				else if(package.dirEnum == (byte)DirectoryEnum.LocalAppData) {
					basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Configs.Dir_LocalApp_Folder);
				}

				// If the basePath wasn't set (shouldn't ever happen), just end here.
				if(basePath.Length == 0) {
					throw new Exception("The basePath was not set correctly due to invalid .dirEnum.");
				}

				string toPath = package.finalPath.Length > 0 ? Path.Combine(basePath, package.finalPath) : basePath;

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

				// Update the Versioning.txt File
				VersioningData localVersion = Installation.GetLocalVersioningData();
				localVersion.UpdatePackage(package);

				Installation.SaveVersionData(localVersion);

				// Update the Creo Launcher's Label:
				WebClient webClient = new WebClient();
				string curVersion = webClient.DownloadString(Configs.BucketURL + Configs.VersionFile);

				// Update Main Window
				MainWindow.WindowRef.SetVersionLabel(curVersion);
				MainWindow.WindowRef.SetStatus(LaunchStatus.Ready);
				MainWindow.WindowRef.StatusShow("Updates Installed!", 33, 163, 37, 255);
			} catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error from installing update: {ex}");
			}
		}
	}
}
