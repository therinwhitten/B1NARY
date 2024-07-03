namespace B1NARY.DataPersistence
{
	using B1NARY.Scripting;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using OVSSerializer;
	using B1NARY.CharacterManagement;
	using B1NARY.Audio;
	using B1NARY.UI.Colors;
	using System.Threading.Tasks;
	using Stopwatch = System.Diagnostics.Stopwatch;
	using System.Linq;
	using OVSSerializer.IO;
	using HDConsole;
	using System.Text;
	using System.IO.Pipes;
	using System.Xml;
	using OVSSerializer.Extras;
	using System.Linq.Expressions;

	public enum Gender : byte
	{
		Male = 0,
		Female = 1,
	}

	public class SaveSlot
	{
		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("save_active", (args) =>
			{
				if (args.Length == 0)
				{
					SaveSlot mainSlot = ActiveSlot;
					StringBuilder line = new($"<b>{mainSlot.SaveName}</b>");
					line.AppendLine($"\n\tpath: {mainSlot.metadata.DirectoryInfo.FullPath}");
					if (mainSlot.strings.Count > 0)
					{
						line.AppendLine("strings:\n\t");
						line.AppendJoin("\n\t", mainSlot.strings.Keys);
					}
					if (mainSlot.booleans.Count > 0)
					{
						line.AppendLine("booleans:\n\t");
						line.AppendJoin("\n\t", mainSlot.booleans.Keys);
					}
					return;
				}
				throw new NotImplementedException();

			}) { description = "Gets the existing save slot, or sets a new saveslot from an existing save list.", optionalArguments = { "save name" } },

			new HDCommand("save_quicksave", args => Quicksave()) { description = "Saves the save into the saved saves list" },
		};

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
		public static OSDirectory PersistentData
		{
			get
			{
				try { return m_persist = new OSDirectory(Application.persistentDataPath); }
				catch { return m_persist; }
			}
		}
		private static OSDirectory m_persist;

		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static OSDirectory StreamingAssets
		{
			get
			{
				try { return m_streaming = new OSDirectory(Application.streamingAssetsPath); }
				catch { return m_streaming; }
			}
		}
		private static OSDirectory m_streaming;

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
		public static OSDirectory SavesDirectory => PersistentData.GetSubdirectories("Saves");

		public static OVSXmlSerializer<SaveSlot> SlotSerializer { get; } =
		new OVSXmlSerializer<SaveSlot>()
		{
			TypeHandling = IncludeTypes.SmartTypes,
			VersionLeniency = Versioning.Leniency.All,
			Version = new Version(2, 0, 0),
			IndentChars = "\t",
			Indent = true,
			IgnoreUndefinedValues = true,
		};

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
		public static void Quicksave()
		{
			if (!PlayerConfig.Instance.quickSaveOverrides.Value)
				ActiveSlot.metadata.ChangeFileTo(null);
			ActiveSlot.Save();
		}

		public static SaveSlot LoadIntoMemory(OSFile loadSlot)
		{
			using FileStream fileStream = loadSlot.OpenRead();
			SaveSlot slot = SlotSerializer.Deserialize(fileStream);
			slot.metadata.ChangeFileTo(loadSlot);
			return slot;
		}

		public static IReadOnlyList<KeyValuePair<OSFile, Lazy<SaveSlot>>> AllSaves
		{
			get
			{
				if (m_saves == null)
				{
					OSFile[] array = SavesDirectory.GetFiles();
					var files = new List<KeyValuePair<OSFile, Lazy<SaveSlot>>>(array.Length);
					for (int i = 0; i < array.Length; i++)
					{
						OSFile currentFile = array[i];
						Lazy<SaveSlot> lazy = new(() => LoadIntoMemory(currentFile));
						if (currentFile.Name.Contains(NAME_START) && currentFile.Extension.Contains(NAME_EXT))
							files.Add(new KeyValuePair<OSFile, Lazy<SaveSlot>>(currentFile, lazy));
					}
					m_saves = files;
				}
				return m_saves;
			}
		}
		private static IReadOnlyList<KeyValuePair<OSFile, Lazy<SaveSlot>>> m_saves;
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
			$"{metadata.lastSaved}";

		[field: OVSXmlAttribute("name")]
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

		[OVSXmlIgnore]
		public bool hasSaved = false;

		public SaveSlot()
		{
			metadata = new Metadata();
			booleans = new Collection<bool>();
			strings = new Collection<string>();
		}

		public void Save()
		{
			FileStream fileStream = null;
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				bool completedTask = false;
				//Debug.Log($"Starting save '{SaveName}' into {metadata.DirectoryInfo.FullPath}..");
				SceneManager.Instance.StartCoroutine(MainThread());

				hasSaved = true;
				//Debug.Log($"Defining metadata of {SaveName}..");
				metadata.lastSaved = DateTime.Now;
				//Debug.Log($"Now retrieving scene data for {SaveName}..");
				scriptPosition = ScriptPosition.Define();
				characterSnapshots = ActorSnapshot.GetCurrentSnapshots();
				audio = SerializedAudio.SerializeAudio();
				formatName = ColorFormat.ActiveFormat.FormatName;

				//Debug.Log($"Getting thumbnail for {SaveName}..");
				byte[] thumbnail = ScreenCapture.CaptureScreenshotAsTexture().EncodeToJPG();
				Task.Run(() =>
				{
					//Debug.Log($"Thumbnail created for {SaveName}, now encrypting & compressing..");
					metadata.thumbnail = new Thumbnail(new Vector2Int(128, 128), thumbnail);
					//Debug.Log($"Serializing '{SaveName}' filepath {metadata.DirectoryInfo.FullPath}..");
					fileStream = metadata.DirectoryInfo.Create();

				}).ContinueWith((task) =>
				{
					completedTask = true;
					if (task.IsFaulted)
						DisplayException(task.Exception);
					//else
					//    Debug.Log($"Saving finalized! Waiting to serialize on main loop. \n{stopwatch.Elapsed}");
				});

				IEnumerator MainThread()
				{
					yield return new WaitUntil(() => completedTask);
					SlotSerializer.Serialize(fileStream, this);
					fileStream.Dispose();
					//Debug.Log($"Saving finished! {stopwatch.Elapsed}\nClearing save cache..");
					stopwatch.Stop();
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
				ColorFormat.SetFormat(formatName);
			};
			wrapper.AfterActions += (mono) => ScriptHandler.Instance.NextLine();
			wrapper.Start();
		}
		/// <summary>
		/// Data that mainly concerns around the file itself and B1NARY.
		/// </summary>
		public class Metadata
		{
			public OSFile DirectoryInfo
			{
				get
				{
					if (string.IsNullOrEmpty(m_directoryInfo))
					{
						OSFile returnVal = SavesDirectory.GetFileIncremental(NAME_START + NAME_EXT, true);
						m_directoryInfo = returnVal.FullPath;
						return returnVal;
					}
					return new OSFile(m_directoryInfo);
				}
			}
			public void ChangeFileTo(OSFile fileInfo, bool deleteOnMove = false)
			{
				if (deleteOnMove && File.Exists(m_directoryInfo))
					File.Delete(m_directoryInfo);
				m_directoryInfo = fileInfo?.FullPath;
			}
			[OVSXmlIgnore]
			private string m_directoryInfo;
			public DateTime lastSaved;
			public Thumbnail thumbnail;
		}

		// Unlockable items
		internal CollectibleCollection collectibles = new();
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

	/// <summary>
	/// Stores flags for unlocking various things.
	/// </summary>
	/// <param name="Gallery"></param>
	/// <param name="Map"></param>
	/// <param name="Chars"></param>
	[Serializable]
	public record CollectibleCollection(List<string> Gallery, List<string> Map, List<string> CharacterProfiles) : IOVSXmlSerializable
	{
		public record NewFlag(string Type, string FlagName, string FormalName);
		[Command("bny_unlock_unlockable")]
		public static void UnlockUnlockable(string type, string flagName, string formalName)
		{
			type = type.ToLower();
			if (SaveSlot.ActiveSlot == null)
			{
				// Missing savefile, saving directly to config instead.
				HashSet<string> saveTo = type switch
				{
					UNLOCKED_GALLERY_KEY => PlayerConfig.Instance.collectibles.Gallery,
					UNLOCKED_MAP_KEY => PlayerConfig.Instance.collectibles.Map,
					UNLOCKED_CHAR_KEY => PlayerConfig.Instance.collectibles.CharacterProfiles,
					_ => throw new InvalidOperationException($"type '{type}' is not valid!")
				};
				saveTo.Add(flagName);
				return;
			}
			// Saving onto existing savefile
			List<string> target = type switch
			{
				UNLOCKED_GALLERY_KEY => SaveSlot.ActiveSlot.collectibles.Gallery,
				UNLOCKED_MAP_KEY => SaveSlot.ActiveSlot.collectibles.Map,
				UNLOCKED_CHAR_KEY => SaveSlot.ActiveSlot.collectibles.CharacterProfiles,
				_ => throw new InvalidOperationException($"type '{type}' is not valid!")
			};
			if (target.Contains(flagName))
				return;
			target.Add(flagName);
			UnlockedUnlockableEvent?.Invoke(new(type, flagName, formalName));
		}
		public static event Action<NewFlag> UnlockedUnlockableEvent;
		//Sub Label for Unlockables
		public const string UNLOCKED_GALLERY_KEY = "gallery";
		public const string UNLOCKED_MAP_KEY = "map";
		public const string UNLOCKED_CHAR_KEY = "charprofile";

		public CollectibleCollection() : this(new(), new(), new()) { }

		bool IOVSXmlSerializable.ShouldWrite => true;
		void IOVSXmlSerializable.Read(XmlNode value)
		{
			Gallery.AddRange(Read(UNLOCKED_GALLERY_KEY)); 
			Map.AddRange(Read(UNLOCKED_MAP_KEY)); 
			CharacterProfiles.AddRange(Read(UNLOCKED_CHAR_KEY));
			string[] Read(string name)
			{
				try { return value.ChildNodes.FindNamedNode(name).InnerText.Split(','); }
				catch { return Array.Empty<string>(); }
			}
		}
		void IOVSXmlSerializable.Write(XmlNode currentNode)
		{
			XmlDocument document = currentNode.OwnerDocument;
			Merge(Gallery, UNLOCKED_GALLERY_KEY); 
			Merge(Map, UNLOCKED_MAP_KEY); 
			Merge(CharacterProfiles, UNLOCKED_CHAR_KEY);
			void Merge(IEnumerable<string> strings, string name)
			{
				XmlElement element = document.CreateElement(name);
				element.InnerText = string.Join(',', strings);
				currentNode.AppendChild(element);
			}
		}
	}
}
