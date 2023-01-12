namespace B1NARY.DataPersistence
{
	using HideousDestructor.DataPersistence;
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
	using System.Text;

	[Serializable]
	public class SaveSlot : SerializableSlot, IDisposable, IDeserializationCallback
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

		/// <summary>
		/// Saves the game using <see cref="Instance"/>'s <see cref="About.fileName"/>
		/// </summary>
		public static void SaveGame()
		{
			Instance.Serialize();
		}
		public static void SaveGame(int index)
		{
			Instance.Name = StartingName + index;
			SaveGame();
		}
		public static void SaveGame(string name)
		{
			Instance.Name = name;
			SaveGame();
		}
		/// <summary>
		/// Loads a game through
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static void LoadGame(int index) =>
			LoadGame(Deserialize<SaveSlot>(FilePath(StartingName + index)));
		public static void LoadGame(string name) =>
			LoadGame(Deserialize<SaveSlot>(FilePath(name)));
		/// <summary>
		/// Loads the previous state of the current <see cref="SaveSlot"/> in the
		/// game files.
		/// </summary>
		public static void QuickLoad() =>
			LoadGame(Deserialize<SaveSlot>(Instance.fileInfo));
		public static void LoadGame(SaveSlot slot)
		{
			Instance = slot;
			Instance.Load();
		}


		/// <summary>
		/// Completes <see cref="SavesDirectory"/> by adding <see cref="extension"/>
		/// and the <paramref name="saveName"/>.
		/// </summary>
		/// <param name="saveName"> The fileData. </param>
		/// <returns> The file fileInfo about the save file. May be non-existant. </returns>
		private static FileInfo FilePath(object saveName) =>
			PersistentData.GetFile($"{saveName}{extension}");
		/// <summary>
		/// Retrieves a collection that is found in the saves folder.
		/// </summary>
		public static IReadOnlyList<SaveSlot> AllFiles
		{
			get
			{
				if (m_files == null)
				{
					FileInfo[] files = PersistentData.GetFiles();
					var slots = new List<SaveSlot>(files.Length);
					for (int i = 0; i < files.Length; i++)
						if (files[i].Extension == extension)
							slots.Add(Deserialize<SaveSlot>(files[i]));
					m_files = slots;
				}
				return m_files;
			}
			set => m_files = value;
		}
		private static IReadOnlyList<SaveSlot> m_files;


		public Data data;
		public ScriptPosition scriptPosition;


		private void RefreshOnScene()
		{
			data.choice.Clear();
		}


		public SaveSlot() : base(FilePath($"{StartingName}{AllFiles.Count}"))
		{
			data = new Data();
			OnDeserialization(this);
		}
		public void OnDeserialization(object sender)
		{
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

		/// <summary>
		/// Shows the contents of the slot file to the user.
		/// </summary>
		/// <returns></returns>
		public string UserContents =>
				// Shows script path
				$"\"{ScriptHandler.ToVisual(scriptPosition.documentPath.FullName)}\""
				// Adds the player name to same line
				+ $" : {data.PlayerName}"
				// Shows the last saved on next line
				+ $"\n{LastSaved}"
				// Shows the time played on the next line
				+ $"\n{TimeUsed}";
		public override string ToString()
		{
			return base.ToString();
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