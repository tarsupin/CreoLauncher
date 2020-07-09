using System;
using System.IO;

namespace CreoLauncher {

	// Local Dir: C:\Users\MyUser\AppData\Local\NexusGames\Creo
	// Roaming Dir: C:\Users\MyUser\AppData\Roaming\NexusGames\Creo

	public class FilesLocal {

		public readonly string localDir;            // Points to your Local Directory.
		public readonly string roamingDir;          // Points to your Roaming Directory.

		public FilesLocal() {
			this.localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NexusGames/Creo");
			this.roamingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NexusGames/Creo");

			// Ensure Local & Roaming Root Directories Exist.
			this.MakeDirectory("");
			this.MakeDirectory("", false);
		}

		public bool FileExists(string localPath, bool localDir = true) {
			string filePath = Path.GetFullPath(Path.Combine(localDir == true ? this.localDir : this.roamingDir, localPath));
			return File.Exists(filePath);
		}

		public void MakeDirectory(string dirName, bool localDir = true) {
			string filePath = Path.GetFullPath(Path.Combine(localDir == true ? this.localDir : this.roamingDir, dirName));

			// Create Directory if it doesn't exist.
			if(!Directory.Exists(filePath)) {
				Directory.CreateDirectory(filePath);
				Console.WriteLine("Creating Local Directory: " + filePath);
			}
		}

		public void WriteFile(string localPath, string content, bool localDir = true) {
			string filePath = Path.GetFullPath(Path.Combine(localDir == true ? this.localDir : this.roamingDir, localPath));
			File.WriteAllText(filePath, content);
		}

		public string ReadFile(string localPath, bool localDir = true) {
			string filePath = Path.GetFullPath(Path.Combine(localDir == true ? this.localDir : this.roamingDir, localPath));
			return File.ReadAllText(filePath);
		}

		public string LocalFilePath(string localPath, bool localDir = true) {
			return Path.GetFullPath(Path.Combine(localDir == true ? this.localDir : this.roamingDir, localPath));
		}
	}
}
