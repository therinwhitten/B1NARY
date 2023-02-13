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
	using System.Xml.Serialization;
	using XmlSerializer = HideousDestructor.DataPersistence.XmlSerializer;
	using System.Threading.Tasks;

	public class SaveSlot
	{
		public const string StartingName = "Slot" + Extension,
			Extension = ".xml";

		public static DirectoryInfo SavesDirectory { get; } =
			SerializableSlot.PersistentData.CreateSubdirectory("Saves");

		public static SaveSlot ActiveSlot { get; private set; }
		public static SaveSlot PassivelyLoadSlot(SaveSlot saveSlot)
		{
			if (ActiveSlot != null)
				ActiveSlot.Save();
			ActiveSlot = saveSlot;
			return saveSlot;
		}
		public static SaveSlot LoadSlot(SaveSlot saveSlot)
		{
			PassivelyLoadSlot(saveSlot);
			saveSlot.Load();
			return saveSlot;
		}

		public static IEnumerable<(FileInfo location, Lazy<SaveSlot> slot)> AllSlots
		{
			get
			{
				if (!(m_allSlots is null)) 
					return m_allSlots;
				IEnumerable<FileInfo> info = SavesDirectory.EnumerateFiles()
					.Where(file => file.Extension == Extension);
				return m_allSlots = info.Select(file => (file, new Lazy<SaveSlot>(() => XmlSerializer.Deserialize<SaveSlot>(file))));
			}
		}
		private static IEnumerable<(FileInfo location, Lazy<SaveSlot> slot)> m_allSlots;
		public static bool Refresh()
		{
			if (m_allSlots is null)
				return false;
			m_allSlots = null;
			return true;
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Init()
		{
			GameObject obj = new GameObject("yes yes");
			var behaviour = obj.AddComponent<Marker>();
			behaviour.StartCoroutine(suff());
			IEnumerator suff()
			{
				yield return new WaitForEndOfFrame();
				ActiveSlot = new SaveSlot();
				ActiveSlot.Save();
			}
		}

		// Basic Fileinfo
		[XmlIgnore]
		public FileInfo FileLocation
		{
			get
			{
				if (m_fileLocationInfo is null)
				{
					if (string.IsNullOrEmpty(m_fileLocation))
						return null;
					return FileLocation = new FileInfo(m_fileLocation);
				}
				return m_fileLocationInfo;
			}
			set
			{
				m_fileLocationInfo = value;
				m_fileLocation = value.FullName;
			}
		}
		[XmlIgnore]
		private FileInfo m_fileLocationInfo;
		private string m_fileLocation;

		public Thumbnail thumbnail;
		public ScriptPosition scriptPosition;
		public ScriptDocumentInterface ScriptDocumentInterface
		{
			get
			{
				if (m_scriptDocumentInterface is null)
					m_scriptDocumentInterface = ScriptDocumentInterface.New();
				return m_scriptDocumentInterface;
			}
		}
		private ScriptDocumentInterface m_scriptDocumentInterface;

		public SaveSlot()
		{
			
		}

		public virtual void Save()
		{
			if (FileLocation == null)
				FileLocation = SavesDirectory.GetFileIncremental(StartingName, true);
			try
			{
				thumbnail = Thumbnail.CreateWithScreenshot();
			} catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			scriptPosition = ScriptPosition.Define();
			XmlSerializer.Serialize(FileLocation, this);
			Debug.Log("Saved Slot");
		}
		public virtual void Load()
		{

		}
	}
	[Serializable]
	public sealed class ScriptPosition
	{
		public static ScriptPosition Define()
		{
			return new ScriptPosition()
			{
				sceneName = SceneManager.ActiveScene.name,

			};
		}
		public string sceneName;

		public ScriptPosition()
		{
			
		}
	}
	/*
	/// <summary>
	/// An instance of data about a current spot in a story, storing various 
	/// dictionaries and choices
	/// </summary>
	[Serializable]
	public class SaveSlot// : SerializableSlot, IDisposable, IDeserializationCallback
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
		}
		private static SaveSlot m_instance;
		public static Action<SaveSlot> NewSlotChanged;
		public static bool LoadingSave { get; private set; } = false;

		public static void SaveGame()
		{
			Instance.Serialize();
		}
		/// <summary>
		/// Loads a game through a index
		/// </summary>
		/// <param name="index">the index number, should start at 0. </param>
		public static void LoadGame(int index) =>
			LoadGame(Deserialize<SaveSlot>(FilePath(StartingName + index)));
		/// <summary>
		/// Loads the game with the name.
		/// </summary>
		/// <param name="name"> The name of the file. </param>
		public static void LoadGame(string name) =>
			LoadGame(Deserialize<SaveSlot>(FilePath(name)));
		/// <summary>
		/// Loads the previous state of the current <see cref="SaveSlot"/> in the
		/// game files.
		/// </summary>
		public static void QuickLoad() =>
			LoadGame(Deserialize<SaveSlot>(Instance.fileInfo));
		/// <summary>
		/// Sets <see cref="Instance"/> with <paramref name="slot"/> and loads it.
		/// </summary>
		public static void LoadGame(SaveSlot slot)
		{
			Instance = slot;
			Instance.Load();
		}



		public static DirectoryInfo SavedData { get; } =
			PersistentData.CreateSubdirectory("Saved");
		/// <summary>
		/// Completes <see cref="SavesDirectory"/> by adding <see cref="extension"/>
		/// and the <paramref name="saveName"/>.
		/// </summary>
		/// <param name="saveName"> The fileData. </param>
		/// <returns> The file fileInfo about the save file. May be non-existant. </returns>
		private static FileInfo FilePath(object saveName) =>
			SavedData.GetFile($"{saveName}{extension}");
		/// <summary>
		/// Retrieves a collection that is found in the saves folder.
		/// </summary>
		public static IReadOnlyList<SaveSlot> AllFiles
		{
			get
			{
				if (m_files == null)
				{
					FileInfo[] files = SavedData.GetFiles();
					var slots = new List<SaveSlot>(files.Length);
					for (int i = 0; i < files.Length; i++)
						if (files[i].Extension == extension)
							slots.Add(Deserialize<SaveSlot>(files[i]));
					Debug.Log($"Slots: {slots.Count}\n" + string.Join(",\n", files.Select(info => $"{info.Name}: {info.Extension == extension}")));
					m_files = slots;
				}
				return m_files;
			}
			set => m_files = value;
		}
		private static IReadOnlyList<SaveSlot> m_files;
		public static FileInfo AvailableSlot => SavedData.GetFile($"{DateTime.Now.ToString().Replace('/', '-').Replace(':', '-')}{extension}");

		public ScriptDocumentInterface scriptDocumentInterface;
		public ScriptPosition scriptPosition;


		private void RefreshOnScene()
		{
			scriptDocumentInterface.choice.Clear();
		}


		public SaveSlot() : base(AvailableSlot)
		{
			scriptDocumentInterface = new ScriptDocumentInterface();
		}
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
			SceneManager.Instance.SwitchingScenes.AddPersistentListener(RefreshOnScene);
		}

		public override void Serialize()
		{
			scriptPosition = new ScriptPosition();
			m_files = null;
			base.Serialize();
		}

		/// <summary>
		/// Loads the save slot to the game.
		/// </summary>
		public void Load()
		{
			SceneManager.InstanceOrDefault.StartCoroutine(scriptPosition.LoadEnumerator());
		}

		public void Dispose()
		{
			SceneManager.Instance.SwitchingScenes.RemovePersistentListener(RefreshOnScene);
		}

		/// <summary>
		/// Shows the contents of the slot file to the user.
		/// </summary>
		/// <returns></returns>
		public string UserContents =>
			// Shows fileName
			$"\"{fileInfo.NameWithoutExtension()}\""
			// Adds the player name to same line
			+ $"\n{scriptDocumentInterface.PlayerName}"
			// Shows the time played on the next line
			+ $"\n{TimeUsed}";
		public override string ToString()
		{
			return base.ToString();
		}

		/// <summary>
		/// Where it was saved at.
		/// </summary>
		[Serializable]
		public sealed class ScriptPosition
		{
			public readonly string sceneName;
			public readonly FileInfo documentPath;
			public readonly ScriptLine lastLine;
			public ScriptPosition()
			{
				documentPath = ScriptHandler.Instance.ScriptDocument.DocumentPath;
				sceneName = SceneManager.ActiveScene.name;
				lastLine = ScriptHandler.Instance.CurrentLine;
			}
			public IEnumerator LoadEnumerator()
			{
				var enumerator = SceneManager.InstanceOrDefault.ChangeScene(sceneName);
				while (enumerator.MoveNext())
					yield return enumerator.Current;
				ScriptHandler.Instance.InitializeNewScript(documentPath);
				while (ScriptHandler.Instance.CurrentLine != lastLine)
				{
					ScriptHandler.Instance.NextLine();
					yield return new WaitForEndOfFrame();
				}
			}
		}
		
	}
	*/
}