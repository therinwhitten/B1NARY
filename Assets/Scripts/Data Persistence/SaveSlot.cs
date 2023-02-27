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
	using System.Xml.Serialization;

	public class SaveSlot
	{
		public const string StartingName = "Slot" + Extension,
			Extension = ".xml";

		public static DirectoryInfo SavesDirectory { get; } =
			SerializableSlot.PersistentData.CreateSubdirectory("Saves");
		public static XmlSerializer<SaveSlot> SlotSerializer { get; } =
			new XmlSerializer<SaveSlot>(new XmlSerializerConfig()
			{
				TypeHandling = IncludeTypes.SmartTypes,
				indentChars = "\t",
				indent = true
			});

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
				return null;//return m_allSlots = info.Select(file => (file, new Lazy<SaveSlot>(() => XmlSerializer.Deserialize<SaveSlot>(file))));
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
			using (var stream = FileLocation.Open(FileMode.Create, FileAccess.Write))
				SlotSerializer.Serialize(stream, this, "SaveSlot");
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
				m_sceneName = SceneManager.ActiveScene.name,

			};
		}
		// Current version of OVSXmlSerializer doesn't like auto-implemented properties
		public string SceneName => m_sceneName;
		private string m_sceneName;

		public ScriptPosition()
		{
			
		}
	}
}