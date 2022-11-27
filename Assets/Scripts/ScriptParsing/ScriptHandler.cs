namespace B1NARY.Scripting
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Diagnostics;
	using UnityEngine.InputSystem;
	using B1NARY.UI;
	using B1NARY.Audio;
	using B1NARY.DesignPatterns;
	using B1NARY.DataPersistence;
	using CharacterController = CharacterManagement.CharacterController;
	using System.Collections.ObjectModel;

	[AddComponentMenu("B1NARY/Script Handler")]
	public class ScriptHandler : Singleton<ScriptHandler>
	{
		public static CommandArray[] AllCommands => 
			new CommandArray[]
			{
				Commands,
				AudioController.Commands,
				SceneManager.Commands,
				CharacterController.Commands,
				TransitionManager.Commands,
				UIThemeHandler.Commands
			};
		public static List<string> GetFullDocumentsPaths(string currentPath)
		{
			var output = new List<string>(Directory.GetFiles(currentPath).Where(path => path.EndsWith(".txt")));
			IEnumerable<string> directories = Directory.GetDirectories(currentPath);
			if (directories.Any())
			{
				IEnumerator<string> enumerator = directories.GetEnumerator();
				while (enumerator.MoveNext())
					output.AddRange(GetFullDocumentsPaths(enumerator.Current));
			}
			return output;
		}
		public static List<string> GetFullDocumentsPath()
		{
			return GetFullDocumentsPaths(BasePath);
		}
		public static List<string> GetVisualDocumentsPaths(in List<string> fullPaths)
		{
			var newList = new List<string>(fullPaths);
			for (int i = 0; i < newList.Count; i++)
				newList[i] = ToVisual(fullPaths[i]);
			return newList;
		}
		public static List<string> GetVisualDocumentsPaths()
		{
			return GetVisualDocumentsPaths(GetFullDocumentsPaths(BasePath));
		}
		public static string ToVisual(string path) => path.Replace(BasePath, "").Replace(".txt", "");

		public static string BasePath => $"{Application.streamingAssetsPath}/Docs/";
		/// <summary>
		/// All script-based commands for the script itself and the 
		/// <see cref="DialogueSystem"/>
		/// </summary>
		public static readonly CommandArray Commands = new CommandArray()
		{
			["additive"] = (Action<string>)(boolRaw =>
			{
				bool setting;
				boolRaw = boolRaw.ToLower().Trim();
				if (ScriptDocument.enabledHashset.Contains(boolRaw))
					setting = true;
				else if (ScriptDocument.disabledHashset.Contains(boolRaw))
					setting = false;
				else
					throw new InvalidCastException(boolRaw);
				Instance.scriptDocument.AdditiveEnabled = setting;
			}),
			["changescript"] = (Action<string>)(ChangeScript),
			["usegameobject"] = (Action<string>)(UseGameObject),
			["setbool"] = (Action<string, string>)((name, value) =>
			{
				PersistentData.Instance.Booleans[name] = bool.Parse(value);
			}),
		};

		[ForcePause]
		internal static void ChangeScript(string scriptPath)
		{
			Instance.InitializeNewScript(scriptPath);
		}
		[ForcePause]
		internal static void UseGameObject(string objectName)
		{
			GameObject @object = GameObject.Find(objectName);
			if (@object == null)
				throw new MissingMemberException($"Gameobject '{objectName}' is not found");
			@object.SetActive(true);
			Instance.ShouldPause = true;
			Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => !@object.activeSelf);
				Instance.ShouldPause = false;
				Instance.NextLine();
			}
		}
		/// <summary>
		/// Gets a custom <see cref="ScriptNode"/> based on the requirements.
		/// </summary>
		/// <param name="document"> The document to reference. </param>
		/// <param name="subLines"> 
		/// All the data that the <see cref="ScriptNode"/> is limited to. 
		/// </param>
		/// <returns>
		/// <see langword="null"/> if none of them fits the requirements, 
		/// otherwise returns a <see cref="ScriptNode"/> derived from the class. 
		/// </returns>
		public static ScriptNode GetDefinedScriptNodes(ScriptDocument document, ScriptPair[] subLines, int index)
		{
			if (subLines[0].LineType != ScriptLine.Type.Command)
				return null;
			string command = ScriptLine.CastCommand(subLines[0].scriptLine).command;
			switch (command)
			{
				case "if":
					return new IfBlock(document, subLines, index);
				case "choice":
					return new ChoiceBlock(document, subLines, index);
				case "else":
					return new ElseBlock(document, subLines, index);
			}
			return null;
		}
		/// <summary> Gets the name of the script loaded. </summary>
		public string ScriptName { get; private set; } = string.Empty;
		/// <summary> 
		/// A read-only version of the currently stored 
		/// <see cref="Experimental.ScriptDocument"/>. 
		/// </summary>
		public ScriptDocument ScriptDocument => scriptDocument;
		private ScriptDocument scriptDocument;
		[Tooltip("Where the script starts when it initializes a new script.")]
		public string StartupScriptPath;
		/// <summary>
		/// The player's input.
		/// </summary>
		public PlayerInput playerInput;
		/// <summary>
		/// The names of the keybinds from <see cref="playerInput"/> to make it
		/// go to the <see cref="NextLine"/> on press.
		/// </summary>
		public string[] nextLineButtons;
		/// <summary>
		/// From where the game session starts, or when 
		/// <see cref="InitializeNewScript(string)"/> is called.
		/// </summary>
		public DateTime playedTime;

		/// <summary>
		/// If the game should pause input. Modifying the variable 
		/// can be stacked with other scripts to allow compatibility. This comes 
		/// into play if you a script allows it to continue, but another says it
		/// shouldn't. In this case, it will read <see langword="true"/>.
		/// </summary>
		public bool ShouldPause 
		{ 
			get => m_pauseIterations > 0;
			set 
			{
				const sbyte maxIterations = 12, 
					halfIterations = maxIterations / 2,
					negativeIterations = -maxIterations;
				if (value)
					m_pauseIterations++;
				else
					m_pauseIterations--; 
				if (m_pauseIterations > maxIterations || m_pauseIterations < negativeIterations)
					Utils.ForceCrash(ForcedCrashCategory.Abort);
				if (m_pauseIterations < 0)
					Debug.LogWarning($"The {nameof(ShouldPause)} count is found to be negative, make" +
						" sure to avoid negative values as this will impact other" +
						$" systems! Currently at {m_pauseIterations}");
				else if (m_pauseIterations > halfIterations)
					Debug.LogWarning($"The {nameof(ShouldPause)} is reaching it's" +
						$" limit on the amount iterations it can have, currently" +
						$"at {m_pauseIterations}");
			}
		}
		private sbyte m_pauseIterations = 0;
		/// <summary>
		/// A value that determines if it is running a script and ready to use.
		/// </summary>
		public bool IsActive { get; private set; } = false;
		/// <summary> Gets the current line. </summary>
		public ScriptLine CurrentLine => ScriptDocument != null 
			? scriptDocument.CurrentLine
			: default;
		protected override void SingletonAwake()
		{
			foreach (string key in nextLineButtons)
				playerInput.actions.FindAction(key, true).performed += context => NextLine();
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// A non-static method for <see cref="PersistentData.LoadGame(int)"/>
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public void LoadExistingSave(int index) => PersistentData.Instance.LoadGame(index);
		/// <summary>
		/// A non-static method for <see cref="PersistentData.SaveGame(int)"/>
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public void SaveGame(int index) => PersistentData.Instance.SaveGame(index);
		/// <summary>
		/// Starts a new script from stratch.
		/// </summary>
		/// <param name="scriptPath"> The path of the document. </param>
		public void InitializeNewScript(string streamingAssetsPath = "")
		{
			string finalPath = BasePath + (string.IsNullOrWhiteSpace(streamingAssetsPath) 
				? StartupScriptPath 
				: streamingAssetsPath) + ".txt";
			Clear();
			m_pauseIterations = 0;
			var scriptFactory = new ScriptDocument.Factory(finalPath);
			scriptFactory.AddNodeParserFunctionality(GetDefinedScriptNodes);
			var allCommands = AllCommands;
			for (int i = 0; i < allCommands.Length; i++)
				scriptFactory.commands.AddRange(allCommands[i]);
			scriptDocument = scriptFactory.Parse(false);
			IsActive = true;

			playedTime = DateTime.Now;

			NextLine();
		}
		/// <summary>
		/// Plays lines until it hits a normal dialogue or similar.
		/// </summary>
		/// <returns> The <see cref="ScriptLine"/> it stopped at. </returns>
		public ScriptLine NextLine()
		{
			if (ShouldPause)
			{
				Debug.Log($"Cannot progress to next line due to {nameof(ShouldPause)} is active\n{m_pauseIterations} Instance(s)");
				return default;
			}
			if (scriptDocument == null)
				return default;
			try
			{
				return scriptDocument.NextLine();
			}
			catch (IndexOutOfRangeException)
			{
				IsActive = false;
				throw;
			}
		}

		public bool Clear()
		{
			if (scriptDocument == null)
				return false;
			scriptDocument.Dispose();
			scriptDocument = null;
			IsActive = false;
			return true;
		}
	}
}