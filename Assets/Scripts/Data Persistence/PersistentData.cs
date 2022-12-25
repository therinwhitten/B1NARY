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
		public static void SaveGame()
		{
			FileInfo info = Instance.Serialize();
#if UNITY_EDITOR
			Debug.Log($"Game Saved!\nat path {info.FullName}");
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
			Instance = new PersistentData($"Save{GetAllFiles().Count()}");
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
		public static IOrderedEnumerable<PersistentData> GetAllFiles()
		{
			return SavesDirectory.EnumerateFiles()
				.Where(info => info.Extension == ".sv")
				.Select(info => LoadExistingData(info.Name.Replace(".sv", "")))
				.OrderBy(info => int.Parse(info.fileName.Replace("Save", "")));
		}

		#region About
		public string fileName;
		public BytePNG image;
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

		public PersistentData(string fileName)
		{
			this.fileName = fileName;
			strings = new Dictionary<string, string>();
			PlayerName = string.Empty;
			bools = new Dictionary<string, bool>();
			ints = new Dictionary<string, int>();
			floats = new Dictionary<string, float>();
			choice = new Dictionary<int, ScriptLine>();
			SceneManager.Instance.SwitchingScenes.AddPersistentListener(RefreshOnScene);
			stopwatch = Stopwatch.StartNew();
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
		public FileInfo Serialize()
		{
			lastSaved = DateTime.Now;
			CaptureDocument();
			stopwatch.Stop();
			timePlayed += stopwatch.Elapsed;
			stopwatch = null; 
			var texture = ScreenCapture.CaptureScreenshotAsTexture();
			Instance.image = new BytePNG(texture);
			using (var stream = FilePath(fileName).Open(FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
			stopwatch = Stopwatch.StartNew();
			return FilePath(fileName);
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

	[Serializable]
	public struct BytePNG
	{
		public int width, height;
		public Vector2Int Size => new Vector2Int(width, height);
		public byte[] data;

		public BytePNG(Texture2D texture2D) : this(texture2D.EncodeToPNG(), texture2D.width, texture2D.height)
		{

		}
		public BytePNG(byte[] data, Vector2Int vector2) : this(data, vector2.x, vector2.y)
		{

		}
		public BytePNG(byte[] data, int width, int height)
		{
			this.data = data;
			this.width = width;
			this.height = height;
		}
		public Texture2D ToTexture()
		{
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
			texture.LoadImage(data);
			//bruh.Apply();
			return texture;
		}
	}
}