using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using static CreoLauncher.GamePackage;

namespace CreoLauncher {

	public static class Installation {

		// Paths
		public static string pathRoot;
		public static string pathBuild;
		public static string pathContent;
		public static string pathDownloads;
		public static string pathApp;         // Where the application is stored (in /Build)

		// Packages to Update
		public static GamePackage CurrentPackage;
		public static Dictionary<string, GamePackage> PackagesToUpdate = new Dictionary<string, GamePackage>();
		public static Dictionary<string, bool> PackageDownloaded = new Dictionary<string, bool>();

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

			// Check For Updates
			try {
				string onlineStr;

				using(WebClient client = new WebClient()) {
					onlineStr = client.DownloadString(Configs.BucketURL + Configs.VersioningFile);
				}

				// Retrieve the Online Versioning file.
				VersioningData onlineVersioning = new VersioningData(onlineStr);

				// Loop through every GamePackage, and keep track of which ones need to update:
				foreach(var packages in onlineVersioning.packages) {
					GamePackage onlinePackage = packages.Value;

					// If the Local Versioning doesn't contain this package:
					if(!localVersioning.packages.ContainsKey(packages.Value.title)) {

						// If this package requires an update:
						if(onlinePackage.versionID > 0) {
							PackagesToUpdate.Add(onlinePackage.title, onlinePackage);
						}
					} else {
						GamePackage localPackage = localVersioning.packages[packages.Value.title];

						if(onlinePackage.versionID != localPackage.versionID) {
							PackagesToUpdate.Add(onlinePackage.title, onlinePackage);
						}
					}
				}

				// If there are updates to run:
				if(PackagesToUpdate.Count > 0) {
					Installation.PreparePackageDownloads();
					Installation.RunDownloads();
				}

				// Otherwise, if there are no updates, indicate the Ready state.
				else { MainWindow.WindowRef.SetStatus(LaunchStatus.Ready); }
			}
			
			catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error while checking for updates: {ex}");
			}
		}

		private static void PreparePackageDownloads() {

			// If we're updating the entire /Build foolder, remove sub-packages that don't need updating.
			if(PackagesToUpdate.ContainsKey("Game")) {
				Installation.CheckRemovePackage("App", "Game");
				Installation.CheckRemovePackage("Content", "Game");
				Installation.CheckRemovePackage("Atlas", "Game");
				Installation.CheckRemovePackage("Fonts", "Game");
				Installation.CheckRemovePackage("Images", "Game");
				Installation.CheckRemovePackage("Music", "Game");
				Installation.CheckRemovePackage("Sounds", "Game");
			}

			// If we're updating the entire /Contents folder, remove sub-Content updates:
			if(PackagesToUpdate.ContainsKey("Content")) {
				Installation.CheckRemovePackage("Atlas", "Content");
				Installation.CheckRemovePackage("Fonts", "Content");
				Installation.CheckRemovePackage("Images", "Content");
				Installation.CheckRemovePackage("Music", "Content");
				Installation.CheckRemovePackage("Sounds", "Content");
			}
		}
		
		private static async void RunDownloads() {

			// Loop through each package to update:
			foreach(var ptu in PackagesToUpdate) {
				Installation.CurrentPackage = ptu.Value;
				await InstallPackage(Installation.CurrentPackage);
			}

			// Update the Creo Launcher's Label:
			using(WebClient client = new WebClient()) {
				string curVersion = client.DownloadString(Configs.BucketURL + Configs.VersionFile);
				MainWindow.WindowRef.SetVersionLabel(curVersion);
			}

			MainWindow.WindowRef.SetStatus(LaunchStatus.Ready);
			MainWindow.WindowRef.StatusShow("Updates Installed!", 33, 163, 37, 255);
		}

		private static bool CheckRemovePackage(string PackageToConsider, string PackageToCompare) {
			if(PackagesToUpdate.ContainsKey(PackageToConsider)) {
				if(PackagesToUpdate[PackageToConsider].versionID <= PackagesToUpdate[PackageToCompare].versionID) {
					PackagesToUpdate.Remove(PackageToConsider);
					return true;
				}
			}
			return false;
		}

		private static async Task<bool> InstallPackage(GamePackage package) {
			try {

				// Update Status
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadingUpdate);
				MainWindow.WindowRef.StatusShow("Downloading " + package.title, 89, 240, 33, 40);

				string downloadPath = Path.Combine(Installation.pathDownloads, package.downloadPath);

				using(WebClient client = new WebClient()) {
					client.DownloadProgressChanged += DownloadProgressChanged;
					client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
					await client.DownloadFileTaskAsync(new Uri(Configs.BucketURL + package.downloadPath), downloadPath); // .ContinueWith(t => t.Exception.Message);
				}

				return true;

			} catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error while downloading game updates: {ex}");
			}

			return false;
		}

		// Show the progress of the download in a progress bar.
		private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			Console.WriteLine(e.ProgressPercentage);
		}

		private static void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) {
			
			try {

				// If there was an error from download:
				if(e.Error != null) {
					throw new Exception(e.Error.Message);
				}

				GamePackage package = Installation.CurrentPackage;

				// Assign Package As Download Completed:
				PackageDownloaded[package.title] = true;

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
					
					// Move the file. Overwrite if necessary.
					File.Move(fromPath, toPath, true);
				}

				// Update the Versioning.txt File
				VersioningData localVersion = Installation.GetLocalVersioningData();
				localVersion.UpdatePackage(package);

				Installation.SaveVersionData(localVersion);

			} catch(Exception ex) {
				MainWindow.WindowRef.SetStatus(LaunchStatus.DownloadFailed);
				MainWindow.WindowRef.SetMessageBox($"Error from installing update: {ex}");
			}
		}
	}
}
