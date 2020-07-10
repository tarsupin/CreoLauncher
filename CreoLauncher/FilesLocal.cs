using System;
using System.IO;

namespace CreoLauncher {

	// Local Dir: C:\Users\MyUser\AppData\Local\NexusGames\Creo

	public static class FilesLocal {
		public static string localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NexusGames/Creo");

		public static void VerifyLocalDir() { FilesLocal.MakeDirectory(FilesLocal.localDir); }

		public static bool FileExists(string localPath) {
			string filePath = Path.GetFullPath(Path.Combine(FilesLocal.localDir, localPath));
			return File.Exists(filePath);
		}

		public static void MakeDirectory(string dirName) {
			string filePath = Path.GetFullPath(Path.Combine(FilesLocal.localDir, dirName));

			// Create Directory if it doesn't exist.
			if(!Directory.Exists(filePath)) {
				Directory.CreateDirectory(filePath);
				Console.WriteLine("Creating Local Directory: " + filePath);
			}
		}

		public static void WriteFile(string localPath, string content) {
			string filePath = Path.GetFullPath(Path.Combine(FilesLocal.localDir, localPath));
			File.WriteAllText(filePath, content);
		}

		public static string ReadFile(string localPath) {
			string filePath = Path.GetFullPath(Path.Combine(FilesLocal.localDir, localPath));
			return File.ReadAllText(filePath);
		}

		public static string LocalFilePath(string localPath) {
			return Path.GetFullPath(Path.Combine(FilesLocal.localDir, localPath));
		}
	}
	
	public static class FilesRoaming {
		public static string RoamingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NexusGames/Creo");

		public static void VerifyRoamingDir() { FilesRoaming.MakeDirectory(FilesRoaming.RoamingDir); }

		public static bool FileExists(string RoamingPath) {
			string filePath = Path.GetFullPath(Path.Combine(FilesRoaming.RoamingDir, RoamingPath));
			return File.Exists(filePath);
		}

		public static void MakeDirectory(string dirName) {
			string filePath = Path.GetFullPath(Path.Combine(FilesRoaming.RoamingDir, dirName));

			// Create Directory if it doesn't exist.
			if(!Directory.Exists(filePath)) {
				Directory.CreateDirectory(filePath);
				Console.WriteLine("Creating Roaming Directory: " + filePath);
			}
		}

		public static void WriteFile(string RoamingPath, string content) {
			string filePath = Path.GetFullPath(Path.Combine(FilesRoaming.RoamingDir, RoamingPath));
			File.WriteAllText(filePath, content);
		}

		public static string ReadFile(string RoamingPath) {
			string filePath = Path.GetFullPath(Path.Combine(FilesRoaming.RoamingDir, RoamingPath));
			return File.ReadAllText(filePath);
		}

		public static string RoamingFilePath(string RoamingPath) {
			return Path.GetFullPath(Path.Combine(FilesRoaming.RoamingDir, RoamingPath));
		}
	}
}
