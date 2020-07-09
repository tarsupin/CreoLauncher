
using System.Collections.Generic;

namespace CreoLauncher {

	// Game Packages:
	// If your Package ID is lower than the online Package ID, then you download the package.
	// Each Package has a Download Destination (file path) and a Final Destination (file path) to move to or overwrite.
	// Each Package's ID is tracked in the Package Manager.
	// Each Package has a Title for Statuses.

	public static class PackageManager {

		// Package Tracking Positions - Identifies what delimiter position (and dictionary key) the package is identified with
		public enum PackageKeyIDs : byte {

			// Multiple Packages
			AllData = 0,            // The entire app and all data (content, local data, etc) should be updated.
			AllContent = 1,			// The full Content directory should be updated.
			AllLocal = 2,			// The local files should be updated.

			// Application Packages
			Application = 2,		// The application code needs to be updated.

			// Content Packages
			Atlas = 3,
			Fonts = 4,
			Images = 5,
			Sounds = 6,
			Music = 7,

			// Local Packages
			Planets = 8,
		}

		// Dictionary of Packages
		public static Dictionary<int, GamePackage> packages = new Dictionary<int, GamePackage>();

		public static void LoadPackages(int[] localDetect, int[] onlineDetect) {

			// Application Package
			PackageManager.packages.Add((byte)PackageKeyIDs.Application, new GamePackage("Application", localDetect[(byte)PackageKeyIDs.Application], onlineDetect[(byte)PackageKeyIDs.Application], "Application.zip", ""));

			// Content Packages (Atlas, Fonts, Images, Sounds, Music, etc)
			PackageManager.packages.Add((byte)PackageKeyIDs.Atlas, new GamePackage("Atlas", localDetect[(byte)PackageKeyIDs.Atlas], onlineDetect[(byte)PackageKeyIDs.Atlas], "Atlas.zip", "Content/Atlas"));
			PackageManager.packages.Add((byte)PackageKeyIDs.Fonts, new GamePackage("Fonts", localDetect[(byte)PackageKeyIDs.Fonts], onlineDetect[(byte)PackageKeyIDs.Fonts], "Fonts.zip", "Content/Fonts"));
			PackageManager.packages.Add((byte)PackageKeyIDs.Images, new GamePackage("Images", localDetect[(byte)PackageKeyIDs.Images], onlineDetect[(byte)PackageKeyIDs.Images], "Images.zip", "Content/Images"));
			PackageManager.packages.Add((byte)PackageKeyIDs.Sounds, new GamePackage("Sounds", localDetect[(byte)PackageKeyIDs.Sounds], onlineDetect[(byte)PackageKeyIDs.Sounds], "Sounds.zip", "Content/Sounds"));
			PackageManager.packages.Add((byte)PackageKeyIDs.Music, new GamePackage("Music", localDetect[(byte)PackageKeyIDs.Music], onlineDetect[(byte)PackageKeyIDs.Music], "Music.zip", "Content/Music"));

			// Local Packages
			PackageManager.packages.Add((byte)PackageKeyIDs.Planets, new GamePackage("Planets", localDetect[(byte)PackageKeyIDs.Planets], onlineDetect[(byte)PackageKeyIDs.Planets], "Planets.zip", "Content/Planets"));
		}
	}

	public struct GamePackage {

		public readonly string title;
		public readonly int localID;
		public readonly int onlineID;
		public readonly string downloadPath;
		public readonly string finalPath;

		internal GamePackage(string title, int localID, int onlineID, string downloadPath, string finalPath) {
			this.title = title;
			this.localID = localID;
			this.onlineID = onlineID;
			this.downloadPath = downloadPath;
			this.finalPath = finalPath;
		}

		internal bool NeedsToUpdate() { return this.localID < this.onlineID; }
		internal string GetStatus() { return "Downloading " + this.title; }
	}
}
