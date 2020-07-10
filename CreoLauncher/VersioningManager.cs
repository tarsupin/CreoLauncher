
using System.Collections.Generic;

namespace CreoLauncher {

	// -- Versioning Manager -- /

	// Versioning.txt:		"Images:0:1:Images.zip:/Contents/Images;etc..."
	//	- This contains a Dictionary of the rules for each Versioning Package.
	//	- 1) The Package Title (e.g. "Images", "Game", "Music", etc.
	//	- 2) The Versioning ID to compare to other versions.
	//	- 3) The File to Download from the Server.
	//	- 4) The Base Directory Enum (Root Directory, Build Directory, Local Directory)
	//	- 5) The Destination Directory. Where to unzip or deliver the package / file.

	// If one of your Versioning IDs is lower than the online Versioning ID, then the package needs to be updated.

	// Hardcoded Rules:
	//	- If "Game" package needs updating, it's the only one you need to update. Ignore all others and mark them updated.
	//	- If "Contents" package needs updating, ignore anything with "/Contents" and mark them as updated.

	// Expected Versioning Packages:
	//	- Launcher			// The base directory (and launcher itself) may be updated.
	//	- Game				// The entire /Build folder (the full game) should be updated.
	//	- Contents			// The entire /Contents folder should to be updated.
	//	- Atlas
	//	- Fonts
	//	- Images
	//	- Music
	//	- Sounds
	//	- Local				// The Local Directory (Local AppData) is getting updated.
	//	- Planets			// The Curated Planet List is getting updated.

	public class VersioningManager {

		public Dictionary<string, GamePackage> packages;

		public VersioningManager(string VersioningText) {

			// Split the Versioning Text into its dictionary components.
			string[] split = VersioningText.Split(';');

			// Loop through each option, add it to the dictionary:
			for(byte i = 0; i < split.Length; i++) {

				// Identify the Package Details (extract info via its delimiters)
				string[] packSplit = VersioningText.Split(':');

				// Ignore any package that isn't set correctly. Should have five delimited values.
				if(packSplit.Length < 5) { continue; }

				int versionID = int.Parse(packSplit[1]);
				byte dirEnum = byte.Parse(packSplit[2]);

				this.packages.Add(packSplit[0], new GamePackage(packSplit[0], versionID, dirEnum, packSplit[3], packSplit[4]));
			}
		}
	}

	public struct GamePackage {

		public enum DirectoryEnum : byte {
			RootDirectory = 0,
			ContentDirectory = 1,
			LocalAppData = 2,
		}

		public readonly string title;
		public readonly int versionID;
		public readonly byte dirEnum;			// Enum that indicates the base directory: Root Directory, Content Directory, Local AppData, etc.
		public readonly string downloadPath;	// The server path to download contents FROM. Loads into /downloads/{downloadPath}
		public readonly string finalPath;		// The final relative path to install the package contents to.

		internal GamePackage(string title, int localID, byte dirEnum, string downloadPath, string finalPath) {
			this.title = title;
			this.versionID = localID;
			this.dirEnum = dirEnum;
			this.downloadPath = downloadPath;
			this.finalPath = finalPath;
		}

		public override string ToString() {
			return $"{this.title}:{this.versionID}:{this.dirEnum}:{this.downloadPath}:{this.finalPath}";
		}
	}
}
