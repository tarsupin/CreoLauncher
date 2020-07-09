
namespace CreoLauncher {

	// The Detect.txt file contains ":" for delimiters. 1) Version, 2) Planet Update ID
	// Example: "0.35.0:5"
	struct DetectData {
		internal static DetectData empty = new DetectData(0, 0, 0, 0);
		internal const string emptyStr = "0.0.0:0";

		// Version Numbers
		public readonly short major;
		public readonly short minor;
		public readonly short subMinor;

		// Curated Values
		public readonly int planets;

		internal DetectData(short _major, short _minor, short _subMinor, int _planets) {
			this.major = _major;
			this.minor = _minor;
			this.subMinor = _subMinor;
			this.planets = _planets;
		}

		internal DetectData(string _data) {

			// Split the Data Delimiter.
			string[] split = _data.Split(':');

			if(split.Length < 2) {
				this.major = 0;
				this.minor = 0;
				this.subMinor = 0;
				this.planets = 0;
				return;
			}

			// Get Last Curated Value
			this.planets = int.Parse(split[1]);

			// Get Version
			string[] verSplit = split[0].Split('.');

			if(verSplit.Length < 2) {
				this.major = 0;
				this.minor = 0;
				this.subMinor = 0;
				return;
			}

			this.major = short.Parse(verSplit[0]);
			this.minor = short.Parse(verSplit[1]);
			this.subMinor = verSplit.Length == 2 ? (short) 0 : short.Parse(verSplit[2]);
		}

		internal bool IsEmpty() {
			return this.major == 0 && this.minor == 0 && this.subMinor == 0;
		}
		
		internal bool HasDifferentVersionThan(DetectData otherData) {
			return (this.major != otherData.major || this.minor != otherData.minor || this.subMinor != otherData.subMinor);
		}
		
		public string VersionToString() {
			return $"{this.major}.{this.minor}.{this.subMinor}";
		}

		public override string ToString() {
			return $"{this.major}.{this.minor}.{this.subMinor}:{this.planets}";
		}
	}
}
