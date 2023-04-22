namespace B1NARY.DataPersistence
{
	using HideousDestructor.DataPersistence;
	using B1NARY.Scripting;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization;
	using UnityEngine;
	using System.Linq;
	using OVSXmlSerializer;
	using System.Collections.ObjectModel;
	using B1NARY.CharacterManagement;
	using CharacterManager = B1NARY.CharacterManagement.CharacterManager;
	using B1NARY.Audio;
	using B1NARY.UI.Colors;

	public enum Gender : byte
	{
		Male = 0,
		Female = 1,
	}
	public class SaveSlot
	{
		public const string NAME_START = "SaveSlot_",
			NAME_EXT = ".xml";
		public const string KEY_PLAYER_NAME = "Player Name";
		public const string KEY_ADDITIVE = "Additive";
		public const int MAX_SAVES = 69;
		public static DirectoryInfo SavesDirectory { get; } =
			SerializableSlot.PersistentData.CreateSubdirectory("Saves");
		
		public static XmlSerializer<SaveSlot> SlotSerializer { get; } =
		new XmlSerializer<SaveSlot>(new XmlSerializerConfig()
		{
			TypeHandling = IncludeTypes.SmartTypes,
			VersionLeniency = Versioning.Leniency.Minor,
			Version = new Version(1, 0, 0),
			IndentChars = "\t",
			Indent = true,
		});

		public static SaveSlot ActiveSlot
		{
			get => m_activeSlot;
			set
			{
				m_activeSlot = value;
				if (m_activeSlot != null)
					m_activeSlot.startPlay = DateTime.Now;
			}
		}
		private static SaveSlot m_activeSlot;

		public static SaveSlot LoadIntoMemory(FileInfo loadSlot)
		{
			SaveSlot slot = SlotSerializer.Deserialize(loadSlot);
			slot.metadata.ChangeFileTo(loadSlot);
			slot.metadata.lastSaved = loadSlot.LastWriteTime;
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
						Lazy<SaveSlot> lazy = new Lazy<SaveSlot>(() => LoadIntoMemory(currentFile));
						if (currentFile.Name.Contains(NAME_START) && currentFile.Extension.Contains(NAME_EXT))
							files.Add(new KeyValuePair<FileInfo, Lazy<SaveSlot>>(currentFile, lazy));
					}
					m_saves = files;
				}
				return m_saves;
			}
		}
		private static IReadOnlyList<KeyValuePair<FileInfo, Lazy<SaveSlot>>> m_saves;
		public static void EmptySaveCache() => m_saves = null;

		static SaveSlot()
		{

		}


		public string DisplaySaveContents =>
			$"<size=125%><b>{SaveName}</b></size>\n" +
			$"{PlayerName} : {scriptPosition.SceneName}\n" +
			$"{metadata.lastSaved.ToShortDateString()} : {(metadata.playedAmount.TotalMinutes < 120d ? $"{metadata.playedAmount.TotalMinutes:N1} min" : $"{metadata.playedAmount.TotalHours:N1} hrs")}";

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
		public CharacterSnapshot[] characterSnapshots;
		public SerializedAudio[] audio;
		[XmlIgnore]
		private DateTime startPlay;

		public SaveSlot()
		{
			startPlay = DateTime.Now;
			metadata = new Metadata()
			{
				playedAmount = new TimeSpan(),
			};
			booleans = new Collection<bool>();
			booleans.Add("henable", () => PlayerConfig.Instance.hEnable.Value);
			strings = new Collection<string>();
		}

		public void Save()
		{
			metadata.lastSaved = DateTime.Now;
			metadata.playedAmount += metadata.lastSaved - startPlay;
			startPlay = metadata.lastSaved;
			scriptPosition = ScriptPosition.Define();
			try { metadata.thumbnail = Thumbnail.CreateWithScreenshot(128, 128); }
			catch (Exception ex) { metadata.thumbnail = null; Debug.LogException(ex); }
			characterSnapshots = CharacterSnapshot.GetCurrentSnapshots();
			audio = SerializedAudio.SerializeAudio();
			formatName = ColorFormat.CurrentFormat.FormatName;
			using (var stream = metadata.DirectoryInfo.Open(FileMode.Create, FileAccess.Write))
				SlotSerializer.Serialize(stream, this);
			EmptySaveCache();
		}

		public void Load()
		{
			ActiveSlot = this;
			CoroutineWrapper wrapper = new CoroutineWrapper(ScriptHandler.Instance, scriptPosition.LoadToPosition());
			wrapper.AfterActions += (mono) =>
			{
				for (int i = 0; i < characterSnapshots.Length; i++)
				{
					CharacterSnapshot currentSnapshot = characterSnapshots[i];
					currentSnapshot.Load();
					if (currentSnapshot.selected)
						CharacterManager.Instance.ChangeActiveCharacterViaName(currentSnapshot.gameObjectName);
				}
			};
			wrapper.AfterActions += (mono) =>
			{
				if (AudioController.TryGetInstance(out var controller))
					controller.RemoveAllSounds();
				for (int i = 0; i < audio.Length; i++)
					audio[i].Play();
			};
			wrapper.AfterActions += (mono) => ColorFormat.Set(formatName);
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
				get => string.IsNullOrEmpty(m_directoryInfo)
					? SavesDirectory.GetFileIncremental(NAME_START + NAME_EXT, true)
					: new FileInfo(m_directoryInfo);
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
			[XmlIgnore]
			public DateTime lastSaved;
			public TimeSpan playedAmount;
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
				StreamingAssetsPath = ScriptHandler.DocumentList.ToVisual(ScriptHandler.Instance.document.ReadFile.FullName),
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