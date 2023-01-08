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

	/// <summary>
	/// A controller of the <see cref="Scripting.ScriptDocument"/> in B1NARY.
	/// This handles most of the behaviour of commands and initialization for
	/// ease of use in the standard game, as it can be separated with 
	/// <see cref="ScriptNode"/> to perhaps work on something else.
	/// </summary>
	[AddComponentMenu("B1NARY/Script Handler")]
	public class ScriptHandler : Singleton<ScriptHandler>
	{
		/// <summary>
		/// All the commands that it will use for each document created. Separated
		/// as a regular array due to some script editors wanting to see it.
		/// </summary>
		public static CommandArray[] AllCommands => 
			new CommandArray[]
			{
				Commands,
				AudioController.Commands,
				SceneManager.Commands,
				CharacterController.Commands,
				TransitionManager.Commands,
				ColorFormat.Commands
			};
		/// <summary>
		/// Uses recursion to get all the documents as a full path, including
		/// the drive and such.
		/// </summary>
		/// <param name="currentPath"> The directory to interact with. </param>
		/// <returns> 
		/// All the file paths that start with .txt within the directory.
		/// </returns>
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
		/// <summary>
		/// Uses recursion to get all the documents as a full path, including
		/// the drive and such. This starts with <see cref="BasePath"/> as the
		/// default parameter.
		/// </summary>
		/// <param name="currentPath"> The directory to interact with. </param>
		/// <returns> 
		/// All the file paths that start with .txt within the directory.
		/// </returns>
		public static List<string> GetFullDocumentsPaths()
		{
			return GetFullDocumentsPaths(BasePath);
		}
		/// <summary>
		/// Converts all the paths, expecting coming from 
		/// <see cref="GetFullDocumentsPaths"/>, to convert them to be more visual
		/// and compatible with <see cref="Application.streamingAssetsPath"/>.
		/// This creates a separate list instead of modifying the inputted list
		/// as reference.
		/// </summary>
		/// <param name="fullPaths"> <see cref="GetFullDocumentsPaths"/> </param>
		/// <returns> Gets all the inputted paths as <see cref="ToVisual(string)"/>. </returns>
		public static List<string> GetVisualDocumentsPaths(in List<string> fullPaths)
		{
			var newList = new List<string>(fullPaths.Count);
			for (int i = 0; i < newList.Capacity; i++)
				newList.Add(ToVisual(fullPaths[i]));
			return newList;
		}
		public static List<string> GetVisualDocumentsPaths()
		{
			return GetVisualDocumentsPaths(GetFullDocumentsPaths(BasePath));
		}
		/// <summary>
		/// Converts a full path with drive mentioned, that will be replaced with
		/// <see cref="BasePath"/> and .txt, leaving only the directories that 
		/// start with <see cref="Application.streamingAssetsPath"/>.
		/// </summary>
		public static string ToVisual(string path) => path.Replace(BasePath, "").Replace(".txt", "");
		/// <summary>
		/// Converts a full path with drive mentioned, that will be replaced with
		/// <see cref="BasePath"/> and .txt, leaving only the directories that 
		/// start with <see cref="Application.streamingAssetsPath"/>.
		/// </summary>
		public static string ToVisual(FileInfo path) => path.FullName.Replace(BasePath, "").Replace(path.Extension, "");

		public static string BasePath => $"{Application.streamingAssetsPath.Replace('/', '\\')}\\Docs\\";
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
				SaveSlot.Instance.data.bools[name] = bool.Parse(value);
			}),
			["callremote"] = ((Action<string>)((call) =>
			{
				Instance.ScriptDocument.returnValue = RemoteBlock.CallRemote(Instance.ScriptDocument, call);
			})),
		};

		[ForcePause]
		internal static void ChangeScript(string scriptPath)
		{
			Instance.InitializeNewScript(scriptPath);
		}
		[ForcePause]
		internal static void UseGameObject(string objectName)
		{
			GameObject @object = Marker.FindWithMarker(objectName).SingleOrDefault();
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

		public string FullScriptPath(string docName) =>
			BasePath + (string.IsNullOrWhiteSpace(docName)
				? StartupScriptPath
				: docName) + ".txt";
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

		public void InitializeNewScript(FileInfo file)
		{
			Clear();
			m_pauseIterations = 0;
			var scriptFactory = new ScriptDocument.Factory(file);
			scriptFactory.AddNodeParserFunctionality(
				typeof(IfBlock),
				typeof(ElseBlock),
				typeof(ChoiceBlock),
				typeof(RemoteBlock));
			var allCommands = AllCommands;
			for (int i = 0; i < allCommands.Length; i++)
				scriptFactory.commands.AddRange(allCommands[i]);
			scriptDocument = scriptFactory.Parse(false);
			IsActive = true;

			playedTime = DateTime.Now;

			NextLine();
		}
		/// <summary>
		/// Starts a new script from stratch.
		/// </summary>
		/// <param name="scriptPath"> The path of the document. </param>
		public void InitializeNewScript(string streamingAssetsPath = "")
		{
			string finalPath = FullScriptPath(streamingAssetsPath);
			InitializeNewScript(new FileInfo(finalPath));
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
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using B1NARY.Scripting;
	using System.Diagnostics;

	[CustomEditor(typeof(ScriptHandler))]
	public class ScriptHandlerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var scriptHandler = (ScriptHandler)target;
			List<string> allFullPaths = ScriptHandler.GetVisualDocumentsPaths();
			if (allFullPaths.Count > 0)
			{
				int oldIndex = allFullPaths.IndexOf(scriptHandler.StartupScriptPath);
				if (oldIndex < 0)
				{
					oldIndex = 0;
					scriptHandler.StartupScriptPath = allFullPaths[0];
					EditorUtility.SetDirty(scriptHandler);
				}
				int newIndex = DirtyAuto.Popup(scriptHandler, new GUIContent("Starting Script"), oldIndex, allFullPaths.ToArray());
				if (oldIndex != newIndex)
				{
					scriptHandler.StartupScriptPath = allFullPaths[newIndex];
					EditorUtility.SetDirty(scriptHandler);
				}
				if (GUILayout.Button("Open File"))
					Process.Start(scriptHandler.FullScriptPath(scriptHandler.StartupScriptPath));
			}
			else
			{
				EditorGUILayout.HelpBox("There is no .txt files in the " +
					"StreamingAssets folder found!", MessageType.Error);
			}
			InputActions(scriptHandler);
			if (scriptHandler.IsActive)
				EditorGUILayout.LabelField($"Current Line: {scriptHandler.ScriptDocument.CurrentLine}");
		}
		private void InputActions(in ScriptHandler scriptHandler)
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ScriptHandler.playerInput)));
			if (scriptHandler.playerInput != null)
			{
				EditorGUI.indentLevel++;
				if (GUILayout.Button("Add"))
					Array.Resize(ref scriptHandler.nextLineButtons, scriptHandler.nextLineButtons.Length + 1);
				for (int i = 0; i < scriptHandler.nextLineButtons.Length; i++)
				{
					Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20),
						textEditorRect = new Rect(fullRect) { xMax = fullRect.xMax / 5 * 4 },
						deleteButtonRect = new Rect(fullRect) { xMin = textEditorRect.xMax + 2 };
					try
					{
						scriptHandler.playerInput.actions
						.FindAction(scriptHandler.nextLineButtons[i], true);
					}
					catch
					{
						EditorGUILayout.HelpBox(
						$"'{scriptHandler.nextLineButtons[i]}' may not exist in the player input!",
						MessageType.Error);
					}
					scriptHandler.nextLineButtons[i] = DirtyAuto.Field(textEditorRect,
						scriptHandler, new GUIContent($"Action {i}"),
						scriptHandler.nextLineButtons[i]);
					if (GUI.Button(deleteButtonRect, "Remove"))
					{
						EditorUtility.SetDirty(scriptHandler);
						List<string> newArray = scriptHandler.nextLineButtons.ToList();
						newArray.RemoveAt(i);
						scriptHandler.nextLineButtons = newArray.ToArray();
					}
				}
				EditorGUI.indentLevel--;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif