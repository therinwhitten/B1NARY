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
	using CharacterController = B1NARY.CharacterManagement.CharacterController;

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
			indentChars = "\t",
			indent = true
		});

		public static SaveSlot ActiveSlot { get; set; }

		public static SaveSlot LoadIntoMemory(FileInfo loadSlot)
		{
			SaveSlot slot = SlotSerializer.Deserialize(loadSlot);
			slot.metadata.DirectoryInfo = loadSlot;
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


		public string DisplaySaveContents =>
			$"{PlayerName} : {scriptPosition.SceneName}\n" +
			$"{metadata.lastSaved}\n{metadata.playedAmount}";

		public Metadata metadata;
		public Collection<bool> booleans;
		public string PlayerName
		{
			get => strings.TryGetValue(KEY_PLAYER_NAME, out var str)
				? str
				: "MC";
			set => strings[KEY_PLAYER_NAME] = value;
		}
		public bool Additive
		{
			get => booleans.TryGetValue(KEY_ADDITIVE, out var additive)
				? additive
				: false;
			set => booleans[KEY_ADDITIVE] = value;
		}
		public Collection<string> strings;
		public ScriptPosition scriptPosition;
		public CharacterSnapshot[] characterSnapshots;
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
			strings = new Collection<string>();
		}

		public void Save()
		{
			metadata.lastSaved = DateTime.Now;
			metadata.playedAmount += metadata.lastSaved - startPlay;
			startPlay = metadata.lastSaved;
			scriptPosition = ScriptPosition.Define();
			characterSnapshots = CharacterController.Instance.charactersInScene
				.Select(pair => pair.Value.characterScript.Serialize()).ToArray();
			metadata.thumbnail = Thumbnail.CreateWithScreenshot(128, 128);
			using (var stream = metadata.DirectoryInfo.Open(FileMode.Create, FileAccess.Write))
				SlotSerializer.Serialize(stream, this);
			characterSnapshots = CharacterSnapshot.GetCurrentSnapshots();
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
						CharacterController.Instance.ChangeActiveCharacter(currentSnapshot.gameObjectName);
				}
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
				get => string.IsNullOrEmpty(m_directoryInfo)
					? SavesDirectory.GetFileIncremental(NAME_START + NAME_EXT, true)
					: new FileInfo(m_directoryInfo);
				set
				{
					if (File.Exists(m_directoryInfo))
						File.Delete(m_directoryInfo);
					if (value is null)
						m_directoryInfo = value.FullName;
				}
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