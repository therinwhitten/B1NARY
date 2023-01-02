﻿namespace B1NARY.DataPersistence
{
	using B1NARY.Scripting;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using UnityEngine;
	using Debug = UnityEngine.Debug;
	using Vector2 = UnityEngine.Vector2;
	using System.Drawing;

	[Serializable]
	public class SaveSlot : IDisposable, IDeserializationCallback
	{
		public const string StartingName = "Slot_",
			extension = ".sv";
		// STATIC INTERFACE ----------------------
		/// <summary>
		/// The current save slot loaded. Use helper methods such as <see cref="LoadGame(string)"/>.
		/// To create a new save slot, create a new instance of <see cref="SaveSlot"/>
		/// and assign it to here.
		/// </summary>
		public static SaveSlot Instance
		{
			get => m_instance;
			set
			{
				m_instance?.Dispose();
				m_instance = value;
				NewSlotChanged?.Invoke(m_instance);
			}
		} private static SaveSlot m_instance;
		public static Action<SaveSlot> NewSlotChanged;
		public static bool LoadingSave { get; private set; } = false;

		/// <summary>
		/// Saves the game using <see cref="Instance"/>'s <see cref="About.fileName"/>
		/// </summary>
		public static void SaveGame()
		{
			Instance.Serialize();
		}
		public static void SaveGame(int index)
		{
			Instance.about.fileName = StartingName + index;
			SaveGame();
		}
		public static void SaveGame(string name)
		{
			Instance.about.fileName = name;
			SaveGame();
		}
		/// <summary>
		/// Loads a game through
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static void LoadGame(int index)
		{
			Instance = LoadExistingData(StartingName + index);
			Instance.Load();
		}
		public static void LoadGame(string name)
		{
			Instance = LoadExistingData(name);
			Instance.Load();
		}
		/// <summary>
		/// Loads the previous state of the current <see cref="SaveSlot"/> in the
		/// game files.
		/// </summary>
		public static void QuickLoad()
		{
			Instance = LoadExistingData(Instance.about.fileName);
			Instance.Load();
		}


		/// <summary>
		/// The save file directory.
		/// </summary>
		private static DirectoryInfo SavesDirectory { get; } =
			new DirectoryInfo(Application.persistentDataPath)
			.CreateSubdirectory("Saves");
		/// <summary>
		/// Completes <see cref="SavesDirectory"/> by adding <see cref="extension"/>
		/// and the <paramref name="saveName"/>.
		/// </summary>
		/// <param name="saveName"> The fileData. </param>
		/// <returns> The file info about the save file. May be non-existant. </returns>
		private static FileInfo FilePath(string saveName) =>
			new FileInfo($"{SavesDirectory.FullName}/{saveName}{extension}");
		/// <summary>
		/// Retrieves data without setting instance with <see cref="SavesDirectory"/>
		/// and the <paramref name="name"/>.
		/// </summary>
		/// <param name="name"> The fileData. </param>=
		private static SaveSlot LoadExistingData(string name)
		{
			SaveSlot output;
			using (var stream = FilePath(name).Open(FileMode.Open))
				output = new BinaryFormatter().Deserialize(stream) as SaveSlot;
			return output;
		}
		/// <summary>
		/// Retrieves a collection that is found in the saves folder.
		/// </summary>
		public static IReadOnlyList<SaveSlot> AllFiles
		{
			get
			{
				if (m_files == null)
				{
					FileInfo[] files = SavesDirectory.GetFiles();
					var slots = new List<SaveSlot>(files.Length);
					for (int i = 0; i < files.Length; i++)
						if (files[i].Extension == extension)
							slots.Add(LoadExistingData(files[i].Name.Remove(files[i].Name.LastIndexOf(files[i].Extension))));
					m_files = slots;
				}
				return m_files;
			}
			set => m_files = value;
		}
		private static IReadOnlyList<SaveSlot> m_files;



		public About about;
		public Data data;
		public ScriptPosition scriptPosition;


		private void RefreshOnScene()
		{
			data.choice.Clear();
		}


		public SaveSlot()
		{
			about = new About();
			about.OnDeserialization(this);
			data = new Data();
			OnDeserialization(this);
		}
		public void OnDeserialization(object sender)
		{
			SceneManager.Instance.SwitchingScenes.AddPersistentListener(RefreshOnScene);
		}

		/// <summary>
		/// Saves file to <see cref="SavesDirectory"/>.
		/// </summary>
		public void Serialize()
		{
			about.SetSavedToToday();
			about.UpdateTime();
			scriptPosition = new ScriptPosition();
			about.ImageTexture = ScreenCapture.CaptureScreenshotAsTexture();
			m_files = null;
			using (var stream = FilePath(about.fileName).Open(FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
		}

		/// <summary>
		/// Loads the save slot to the game.
		/// </summary>
		public void Load()
		{
			SceneManager.InstanceOrDefault.StartCoroutine(LoadEnumerator());
			IEnumerator LoadEnumerator()
			{
				var enumerator = SceneManager.InstanceOrDefault.ChangeScene(scriptPosition.sceneIndex);
				while (enumerator.MoveNext())
					yield return enumerator.Current;
				ScriptHandler.Instance.InitializeNewScript(scriptPosition.documentPath);
				while (ScriptHandler.Instance.CurrentLine != scriptPosition.lastLine)
				{
					ScriptHandler.Instance.NextLine();
					yield return new WaitForEndOfFrame();
				}
			}
		}

		public void Dispose()
		{
			SceneManager.Instance.SwitchingScenes.RemovePersistentListener(RefreshOnScene);
		}

		[Serializable]
		public sealed class ScriptPosition
		{
			public readonly int sceneIndex;
			public readonly FileInfo documentPath;
			public readonly ScriptLine lastLine;
			public ScriptPosition()
			{
				documentPath = ScriptHandler.Instance.ScriptDocument.DocumentPath;
				sceneIndex = SceneManager.ActiveScene.buildIndex;
				lastLine = ScriptHandler.Instance.CurrentLine;
			}
		}
		[Serializable]
		public sealed class About : IDeserializationCallback
		{
			public string fileName = $"{StartingName}{AllFiles.Count}";
			public FileInfo SaveLocation => FilePath(fileName);
			public Texture2D ImageTexture
			{
				get
				{
					var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false, false);
					texture.LoadImage(image);
					return texture;
				}
				set
				{
					const float maxSize = 512f;
					image = value.EncodeToJPG();
					using (var stream = new MemoryStream(image))
					{
						Bitmap bitmap = new Bitmap(stream);
						Vector2Int imageRatio = ImageUtility.Ratio(bitmap.Width, bitmap.Height);
						var imageSize = new Vector2Int(bitmap.Width, bitmap.Height);
						// making width to 512, and height as follows
						imageSize.y = (int)(maxSize / imageRatio.x * imageRatio.y);
						imageSize.x = (int)maxSize;
						if (imageSize.y > maxSize)
						{
							imageRatio = ImageUtility.Ratio(imageSize.x, imageSize.y);
							imageSize.x = (int)(maxSize / imageRatio.y * imageRatio.x);
							imageSize.y = (int)maxSize;
						}
						Debug.Log($"{imageSize}");
						Image subImage = bitmap.GetThumbnailImage(imageSize.x, imageSize.y, null, IntPtr.Zero);
						image = (byte[])new ImageConverter().ConvertTo(subImage, typeof(byte[]));
						bitmap.Dispose();
					}
				}
			}
			public byte[] image;

			public void UpdateTime()
			{
				if (stopwatch == null)
					return;
				stopwatch.Stop();
				timePlayed += stopwatch.Elapsed;
				stopwatch.Restart();
			}
			public TimeSpan timePlayed = TimeSpan.Zero;
			public void SetSavedToToday() => lastSaved = DateTime.Now;
			public DateTime lastSaved = default;
			[NonSerialized]
			public Stopwatch stopwatch;


			public void OnDeserialization(object sender)
			{
				stopwatch = Stopwatch.StartNew();
			}
		}
		[Serializable]
		public sealed class Data
		{
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

			public Data()
			{
				strings = new Dictionary<string, string>();
				PlayerName = string.Empty;
				bools = new Dictionary<string, bool>();
				ints = new Dictionary<string, int>();
				floats = new Dictionary<string, float>();
				choice = new Dictionary<int, ScriptLine>();
			}
		}
	}
}