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
			SceneManager.InstanceOrDefault.StartCoroutine(LoadEnumerator());
			IEnumerator LoadEnumerator()
			{
				var enumerator = SceneManager.InstanceOrDefault.ChangeScene(scriptPosition.sceneName);
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
		}
		/// <summary>
		/// Data about B1NARY.
		/// </summary>
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