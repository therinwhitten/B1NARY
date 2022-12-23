namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading.Tasks;
	using UnityEngine;
	using Debug = UnityEngine.Debug;

	[Serializable]
	public class PersistentData : IDisposable
	{
		// STATIC INTERFACE ----------------------
		public static PersistentData Instance 
		{
			get => m_instance;
			set
			{
				m_instance = value;
				NewSlotChanged?.Invoke(m_instance);
			} 
		}
		private static PersistentData m_instance;
		public static Action<PersistentData> NewSlotChanged;
		public static Dictionary<string, bool> Booleans => Instance.bools;
		public static Dictionary<string, string> Strings => Instance.strings;
		public static Dictionary<string, int> Integers => Instance.ints;
		public static Dictionary<string, float> Singles => Instance.floats;
		public static bool IsLoading { get; private set; } = false;


		/// <summary>
		/// Saves data as a <see cref="GameSlotData"/> and writes it into the saves
		/// folder.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void SaveGame(int index = 0)
		{
			Instance.Serialize($"Save{index}");
			Instance.Image = ScreenCapture.CaptureScreenshotAsTexture();
#if DEBUG
			Debug.Log($"Game Saved!\nat path {FilePath($"Save{index}").FullName}");
#endif
		}

		/// <summary>
		/// Load data from Binary format into as a <see cref="GameSlotData"/> and 
		/// writes it back into <see cref="PersistentData"/>.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void LoadGame(int index = 0)
		{
			Instance = LoadExistingData($"Save{index}");
			IsLoading = true;
			Instance.LoadScene();
			IsLoading = false;
			Debug.Log("Game Loaded!");
		}

		/// <summary>
		/// Starts a new gameslot to save the data as a binary format.
		/// </summary>
		public static void CreateNewSlot()
		{
			Instance = new PersistentData();
		}



		private static DirectoryInfo SavesDirectory { get; } =
			new DirectoryInfo(Application.persistentDataPath)
			.CreateSubdirectory("Saves");
		private static FileInfo FilePath(string saveName) => 
			new FileInfo($"{SavesDirectory.FullName}/{saveName}.sv");
		public static PersistentData LoadExistingData(string name)
		{
			PersistentData output;
			using (var stream = FilePath(name).Open(FileMode.Open))
				output = new BinaryFormatter().Deserialize(stream) as PersistentData;
			return output;
		}
		public static IEnumerable<PersistentData> GetAllFiles()
		{
			return SavesDirectory.EnumerateFiles()
				.Where(info => info.Extension == ".sv")
				.Select(info => LoadExistingData(info.Name));
		}

		#region About
		public Texture2D Image
		{
			get
			{
				var texture = new Texture2D(m_image.size.width, m_image.size.height, TextureFormat.RGBA32, false);
				texture.LoadRawTextureData(m_image.data);
				texture.Apply();
				return texture;
			}
			set
			{
				m_image = (value.EncodeToPNG(), (value.width, value.height));
			}
		}
		private (byte[] data, (int width, int height) size) m_image;
		public TimeSpan timePlayed = TimeSpan.Zero;
		public DateTime lastSaved = default;
		[NonSerialized]
		public Stopwatch stopwatch;
		#endregion

		public Dictionary<int, ScriptLine> choice;

		public Dictionary<string, string> strings;
		public Dictionary<string, bool> bools;
		public Dictionary<string, int> ints;
		public Dictionary<string, float> floats;

		public string PlayerName
		{
			get => strings["Player Name"];
			set => strings["Player Name"] = value;
		}
		public int sceneIndex;
		public string documentPath;
		private ScriptLine lastLine;

		public PersistentData()
		{
			strings = new Dictionary<string, string>();
			PlayerName = string.Empty;
			bools = new Dictionary<string, bool>();
			ints = new Dictionary<string, int>();
			floats = new Dictionary<string, float>();
			choice = new Dictionary<int, ScriptLine>();
			SceneManager.Instance.SwitchingScenes.AddPersistentListener(RefreshOnScene);
			stopwatch = Stopwatch.StartNew();
			Debug.Log($"Amogus");
		}
		private void RefreshOnScene()
		{
			choice.Clear();
		}

		public void CaptureDocument()
		{
			documentPath = ScriptHandler.Instance.ScriptDocument.documentPath;
			sceneIndex = SceneManager.ActiveScene.buildIndex;
			lastLine = ScriptHandler.Instance.CurrentLine;
		}
		public void Serialize(string name)
		{
			lastSaved = DateTime.Now;
			CaptureDocument();
			stopwatch.Stop();
			timePlayed += stopwatch.Elapsed;
			stopwatch = null;
			using (var stream = FilePath(name).Open(FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
			stopwatch = Stopwatch.StartNew();
		}
		public void LoadScene()
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
			ScriptHandler.Instance.InitializeNewScript(documentPath);
			while (ScriptHandler.Instance.CurrentLine != lastLine)
				ScriptHandler.Instance.NextLine();
		}
		public void Dispose()
		{
			SceneManager.Instance.SwitchingScenes.RemovePersistentListener(RefreshOnScene);
		}
	}
}