namespace B1NARY.DataPersistence
{
	using B1NARY.Scripting;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using OVSXmlSerializer;
	using B1NARY.CharacterManagement;
	using B1NARY.Audio;
	using B1NARY.UI.Colors;
	using System.Threading.Tasks;
	using Stopwatch = System.Diagnostics.Stopwatch;
	using System.Linq;

	public enum Gender : byte
	{
		Male = 0,
		Female = 1,
	}
	
	public class SaveSlot
	{
		/// <summary>
		/// Gets the local appdata folder for saving settings, configs, etc.
		/// </summary>
		/// <remarks>
		/// Uses window's <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/> 
		/// structure due to <see cref="Application.persistentDataPath"/>
		/// being incredibly buggy, especially when the folder is missing 
		/// from the appdata; causing it to not display any directory location 
		/// at all.
		/// </remarks>
		public static DirectoryInfo PersistentData
		{
			get
			{
				try { return m_persist = new DirectoryInfo(Application.persistentDataPath); }
				catch { return m_persist; }
			}
		}
		private static DirectoryInfo m_persist;

		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static DirectoryInfo StreamingAssets
		{
			get
			{
				try { return m_streaming = new DirectoryInfo(Application.streamingAssetsPath); }
				catch { return m_streaming; }
			}
		}
		private static DirectoryInfo m_streaming;

		public enum QuicksaveType
		{
			/// <summary>
			/// Creates a new save every time the player quicksaves
			/// </summary>
			NewSave,
			/// <summary>
			/// Quicksaves on an existing save, or a new save if it doesn't exist
			/// </summary>
			ExistingSave,
		}
		public const string NAME_START = "SaveSlot_",
			NAME_EXT = ".xml";
		public const string KEY_PLAYER_NAME = "Player Name";
		public const string KEY_ADDITIVE = "Additive";
		public const int MAX_SAVES = 69;
		public static DirectoryInfo SavesDirectory => PersistentData.GetSubdirectory("Saves");
		
		public static XmlSerializer<SaveSlot> SlotSerializer { get; } =
		new XmlSerializer<SaveSlot>(new XmlSerializerConfig()
		{
			TypeHandling = IncludeTypes.SmartTypes,
			VersionLeniency = Versioning.Leniency.Minor,
			Version = new Version(1, 0, 0),
			IndentChars = "\t",
			Indent = true,
			IgnoreUndefinedValues = true,
		});

		public static SaveSlot ActiveSlot
		{
			get => m_activeSlot;
			set
			{
				m_activeSlot = value;
				if (m_activeSlot != null)
				{
					if (!m_activeSlot.booleans.ContainsKey("henable"))
						m_activeSlot.booleans.Add("henable", () => PlayerConfig.Instance.hEnable.Value);
				}
			}
		}
		private static SaveSlot m_activeSlot;
		public void Quicksave()
		{
			if (!PlayerConfig.Instance.quickSaveOverrides.Value)
				ActiveSlot.metadata.ChangeFileTo(null);
			ActiveSlot.Save();
		}

		public static SaveSlot LoadIntoMemory(FileInfo loadSlot)
		{
			loadSlot.Refresh();
			SaveSlot slot = SlotSerializer.Deserialize(loadSlot);
			slot.metadata.ChangeFileTo(loadSlot);
			return slot;
		}

		public static IReadOnlyList<KeyValuePair<FileInfo, Lazy<SaveSlot>>> AllSaves
		{
			get
			{
				if (m_saves == null)
				{
					FileInfo[] array = SavesDirectory.GetFiles();
					var files = new List<KeyValuePair<FileInfo, Lazy<SaveSlot>>>(array.Length);
					for (int i = 0; i < array.Length; i++)
					{
						FileInfo currentFile = array[i];
						Lazy<SaveSlot> lazy = new(() => LoadIntoMemory(currentFile));
						if (currentFile.Name.Contains(NAME_START) && currentFile.Extension.Contains(NAME_EXT))
							files.Add(new KeyValuePair<FileInfo, Lazy<SaveSlot>>(currentFile, lazy));
					}
					m_saves = files;
				}
				return m_saves;
			}
		}
		private static IReadOnlyList<KeyValuePair<FileInfo, Lazy<SaveSlot>>> m_saves;
		public static void EmptySaveCache()
		{
			m_saves = null;
			EmptiedSaveCache?.Invoke();
		}

		public static event Action EmptiedSaveCache;

		static SaveSlot()
		{

		}
		public string DisplaySaveContents =>
			$"<size=125%><b>{SaveName}</b></size>\n" +
			$"{PlayerName} : {scriptPosition.SceneName}\n" +
			$"{metadata.lastSaved.ToString()}";

		[field: XmlAttribute("name")]
		public string SaveName { get; set; } = "QuickSave";
		public Metadata metadata;
		public Collection<bool> booleans;
		public const string DEFAULT_NAME = "MC";
		public string PlayerName
		{
			get => strings.TryGetValue(KEY_PLAYER_NAME, out var str)
				? str
				: DEFAULT_NAME;
			set => strings[KEY_PLAYER_NAME] = value;
		}
		public bool Additive
		{
			get => booleans.TryGetValue(KEY_ADDITIVE, out var additive)
				? additive
				: false;
			set => booleans[KEY_ADDITIVE] = value;
		}

		public const string BINARY_KEY = "n-b";
		public bool IsBinary
		{
			get => booleans.TryGetValue(BINARY_KEY, out var nonbinary)
				? !nonbinary
				: true;
			set => booleans[BINARY_KEY] = !value;
		}
		public const string GENDER_KEY = "MalePath";
		public Gender Gender
		{
			get => booleans.TryGetValue(GENDER_KEY, out var isMale)
				? (isMale ? Gender.Male : Gender.Female)
				: Gender.Male;
			set => booleans[GENDER_KEY] = value == Gender.Male;
		}
		public Collection<string> strings;
		public string formatName;
		public ScriptPosition scriptPosition;
		public ActorSnapshot[] characterSnapshots;
		public List<SerializedAudio> audio;

		[XmlIgnore]
		public bool hasSaved = false;

		public SaveSlot()
		{
			metadata = new Metadata();
			booleans = new Collection<bool>();
			strings = new Collection<string>();
		}

		public void Save()
		{
			try
			{
				bool completedTask = false;
				SceneManager.Instance.StartCoroutine(MainThread());

				hasSaved = true;
				metadata.lastSaved = DateTime.Now;
				Stopwatch stopwatch = Stopwatch.StartNew();
				scriptPosition = ScriptPosition.Define();
				characterSnapshots = ActorSnapshot.GetCurrentSnapshots();
				audio = SerializedAudio.SerializeAudio();
				formatName = ColorFormat.CurrentFormat.FormatName;

				byte[] thumbnail = ScreenCapture.CaptureScreenshotAsTexture().EncodeToJPG();
				Task.Run(() =>
				{
					metadata.thumbnail = new Thumbnail(new Vector2Int(128, 128), thumbnail);
					using MemoryStream tempStream = new();
					SlotSerializer.Serialize(tempStream, this);
					tempStream.Position = 0;
					using FileStream stream = metadata.DirectoryInfo.Open(FileMode.Create, FileAccess.Write);
					tempStream.CopyTo(stream);

				}).ContinueWith((task) =>
				{
					completedTask = true;
					if (task.IsFaulted)
						DisplayException(task.Exception);
					else
						Debug.Log($"Saved! \n{stopwatch.Elapsed}");
					stopwatch.Stop();
				});

				IEnumerator MainThread()
				{
					yield return new WaitUntil(() => completedTask);
					EmptySaveCache();
				}
			}
			catch (Exception ex) { DisplayException(ex); }

			void DisplayException(Exception ex) => Debug.LogException(new InvalidProgramException("Unable to save!", ex));
		}

		public void Load()
		{
			if (!hasSaved)
				throw new InvalidOperationException("Currently active save has not saved properly! Did you press quickload without saving?");
			ActiveSlot = this;
			CoroutineWrapper wrapper = new(ScriptHandler.Instance, scriptPosition.LoadToPosition());
			wrapper.AfterActions += (mono) =>
			{
				for (int i = 0; i < characterSnapshots.Length; i++)
				{
					ActorSnapshot currentSnapshot = characterSnapshots[i];
					if (currentSnapshot.Load(out _))
						if (currentSnapshot.selected)
							CharacterManager.Instance.ChangeActiveCharacterViaCharacterName(currentSnapshot.characterNames.CurrentName);
				}
			};
			wrapper.AfterActions += (mono) =>
			{
				if (AudioController.TryGetInstance(out var controller))
					controller.RemoveAllSounds();
				for (int i = 0; i < audio.Count; i++)
					audio[i].Play();
			};
			wrapper.AfterActions += (mono) =>
			{
				ColorFormat.Set(formatName);
			};
			wrapper.AfterActions += (mono) => ScriptHandler.Instance.NextLine();
			wrapper.Start();
		}
		/// <summary>
		/// Data that mainly concerns around the file itself and B1NARY.
		/// </summary>
		public class Metadata
		{
			public FileInfo DirectoryInfo
			{
				get
				{
					if (string.IsNullOrEmpty(m_directoryInfo))
					{
						FileInfo returnVal = SavesDirectory.GetFileIncremental(NAME_START + NAME_EXT, true);
						m_directoryInfo = returnVal.FullName;
						return returnVal;
					}
					return new FileInfo(m_directoryInfo);
				}
			}
			public void ChangeFileTo(FileInfo fileInfo, bool deleteOnMove = false)
			{
				if (deleteOnMove && File.Exists(m_directoryInfo))
					File.Delete(m_directoryInfo);
				m_directoryInfo = fileInfo?.FullName;
			}
			//public void Rename(in string newName)
			//{
			//	FileInfo dir = DirectoryInfo;
			//	m_directoryInfo = dir.FullName.Replace(dir.NameWithoutExtension(), newName);
			//}
			[XmlIgnore]
			private string m_directoryInfo;
			public DateTime lastSaved;
			public Thumbnail thumbnail;
		}
	}

	[Serializable]
	public struct ScriptPosition
	{
		public static ScriptPosition Define()
		{
			return new ScriptPosition()
			{
				SceneName = SceneManager.ActiveScene.name,
				Line = ScriptHandler.Instance.documentWatcher.CurrentNode.GlobalIndex,
				StreamingAssetsPath = new Document(ScriptHandler.Instance.document.ReadFile).VisualPath,
			};
		}
		public string SceneName { get; set; }
		public string StreamingAssetsPath { get; set; }
		public int Line { get; set; }

		public IEnumerator LoadToPosition()
		{
			var changeSceneEnumerator = SceneManager.Instance.ChangeScene(SceneName, false);
			while (changeSceneEnumerator.MoveNext())
				yield return changeSceneEnumerator.Current;
			ScriptHandler.Instance.NewDocument(StreamingAssetsPath, Line - 1);
		}
	}
}