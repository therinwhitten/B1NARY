namespace B1NARY.Scripting
{
	using B1NARY.Audio;
	using B1NARY.CharacterManagement;
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using B1NARY.UI;
	using B1NARY.UI.Colors;
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;
	using CharacterManager = CharacterManagement.CharacterManager;

	[AddComponentMenu("B1NARY/Script Handler")]
	public sealed class ScriptHandler : Singleton<ScriptHandler>
	{
		internal static readonly ScriptDocumentConfig config;
		static ScriptHandler()
		{
			config = new ScriptDocumentConfig();
			config.AddConstructor(typeof(IfBlock), IfBlock.Predicate);
			config.AddConstructor(typeof(ElseBlock), ElseBlock.Predicate);
			config.AddConstructor(typeof(ChoiceBlock), ChoiceBlock.Predicate);
			config.AddConstructor(typeof(RemoteBlock), RemoteBlock.Predicate);
			config.Commands.AddRange(
				new List<CommandArray>()
				{
					new CommandArray()
					{
						["changescript"] = (Action<string>)(ChangeScript),
						["changescript"] = (Action<string, string>)(ChangeScript),
						["usegameobject"] = (Action<string>)(UseGameObject),
						["setbool"] = (Action<string, string>)((name, value) =>
						{
							SaveSlot.ActiveSlot.booleans[name] = bool.Parse(value);
						}),
						["callremote"] = (Action<string>)((call) =>
						{
							RemoteBlock.CallRemote(Instance.document, call);
						}),
						["throwexception"] = (Action<string>)((message) =>
						{
							throw new Exception(message);
						}),
					},
					DialogueSystem.Commands,
					AudioController.Commands,
					SceneManager.Commands,
					CharacterManager.Commands,
					TransitionManager.Commands,
					ColorFormat.Commands,
					FollowCube.Commands,
				}.SelectMany<CommandArray, OverloadableCommand>(commands => commands));
		}
		[ForcePause]
		internal static void ChangeScript(string scriptPath)
		{
			ScriptHandler handler = Instance;
			handler.NewDocument(scriptPath);
			Debug.Log($"Next document to line: {handler.NextLine()}");
		}
		[ForcePause]
		internal static void ChangeScript(string scriptPath, string line)
		{
			ScriptHandler handler = Instance;
			handler.NewDocument(scriptPath, int.Parse(line) - 1);
			Debug.Log($"Next document to line: {handler.NextLine()}");
		}
		[ForcePause]
		internal static void UseGameObject(string objectName)
		{
			GameObject @object = Marker.FindWithMarker(objectName).SingleOrDefault();
			if (@object == null)
				throw new MissingMemberException($"Gameobject '{objectName}' is not found");
			@object.SetActive(true);
			Instance.pauser.Pause();
			Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => !@object.activeSelf);
				Instance.pauser.Play();
				Instance.NextLine();
			}
		}
		public static DirectoryInfo DocumentFolder { get; } = SerializableSlot.StreamingAssets.GetOrCreateSubDirectory("Docs");
		public static DocumentList AllDocuments { get; } = new DocumentList();



		public Pauser pauser = new Pauser();
		internal ScriptDocument document;
		public bool HasDocument => document != null;
		public bool IsActive { get; private set; } = false;
		[SerializeField]
		internal string defaultStreamingAssetsDocumentPath;
		public IDocumentWatcher documentWatcher;
		public InputSystemUIInputModule input;

		/// <summary>
		/// The player's input.
		/// </summary>
		public PlayerInput playerInput;
		/// <summary>
		/// The names of the keybinds from <see cref="playerInput"/> to make it
		/// go to the <see cref="NextLine"/> on press.
		/// </summary>
		public string[] nextLineButtons;

		private void Awake()
		{
			DontDestroyOnLoad(transform.root);
			for (int i = 0; i < nextLineButtons.Length; i++)
				playerInput.actions.FindAction(nextLineButtons[i], true).performed += (context) => NextLine();
			config.NormalLine += SayLine;
			config.AttributeListeners += ChangeExpression;
			config.EntryListeners += ChangeCharacter;
		}
		private void SayLine(ScriptLine line)
		{
			if (CharacterManager.Instance.ActiveCharacter.HasValue)
				CharacterManager.Instance.ActiveCharacter.Value.controller.SayLine(line);
			else
				throw new NullReferenceException($"There is no active character selected!");
		}

		private void ChangeExpression(string expressionName)
		{
			if (CharacterManager.Instance.ActiveCharacter.HasValue)
				CharacterManager.Instance.ActiveCharacter.Value.controller.CurrentExpression = expressionName;
			else
				throw new NullReferenceException($"There is no active character selected!");
		}

		private void ChangeCharacter(string newCharacter)
		{
			if (!CharacterManager.Instance.ChangeActiveCharacterViaCharacterName(newCharacter))
				throw new MissingMemberException($"'{newCharacter}' is not found in the internal character list!");
		}


		public void NewDocument(int index = 0)
		{
			NewDocument(defaultStreamingAssetsDocumentPath, index);
		}
		public void NewDocument(string streamingAssetsDocument, int index = 0)
		{
			document = new ScriptDocument(config, DocumentList.FromVisual(streamingAssetsDocument));
			documentWatcher = document.StartAtLine(index);
		}

		public ScriptNode NextLine(bool forceContinue = false)
		{
			if (pauser.ShouldPause)
			{
				Debug.Log("Pausing is enabled.");
				return documentWatcher.CurrentNode;
			}
			if (document is null || documentWatcher is null)
			{
				throw new MissingReferenceException("There is no document created in the system!");
			}
			if (!forceContinue && DialogueSystem.TryGetInstance(out var system))
				if (system.IsSpeaking && !DateTimeTracker.IsAprilFools)
				{ 
					DialogueSystem.Instance.StopSpeaking(true);
					return documentWatcher.CurrentNode;
				}
			IsActive = true;
			if (documentWatcher.EndOfDocument)
			{
				if (document.ReadFile != null)
					Debug.LogWarning($"Reached to end of document, '{document.ReadFile}'!");
				else
					Debug.LogWarning($"Reached to end of document!");
				Clear();
				return null;
			}
			documentWatcher.NextNode(out var node);
			return node;
		}


		public void Clear()
		{
			if (!IsActive)
				return;
			IsActive = false;
			document = null;
			documentWatcher = null;
		}



		
		
		public sealed class DocumentList : List<FileInfo>
		{
			public static string ToVisual(string fullPath) =>
				fullPath.Replace(BasePath, "").Replace(".txt", "");
			public static FileInfo FromVisual(string visualPath) =>
				new FileInfo($"{SerializableSlot.StreamingAssets.FullName}/Docs/{visualPath}.txt");

			public static DirectoryInfo DocumentFolder { get; } = SerializableSlot.StreamingAssets.GetOrCreateSubDirectory("Docs");
			private WeakReference visualNames;
			public DocumentList() : base()
			{
				RecursivelyGetFiles(DocumentFolder);
			}
			/// <summary>
			/// Uses recursion to get all the documents as a full path, including
			/// the drive and such.
			/// </summary>
			/// <param name="currentPath"> The directory to interact with. </param>
			/// <returns> 
			/// All the file paths that start with .txt within the directory.
			/// </returns>
			private void RecursivelyGetFiles(DirectoryInfo currentPath)
			{
				AddRange(currentPath.EnumerateFiles().Where(path => path.Extension == ".txt"));
				IEnumerable<DirectoryInfo> directories = currentPath.EnumerateDirectories();
				if (directories.Any())
				{
					using (IEnumerator<DirectoryInfo> enumerator = directories.GetEnumerator())
						while (enumerator.MoveNext())
							RecursivelyGetFiles(enumerator.Current);
				}
			}
			public static string BasePath => $"{Application.streamingAssetsPath.Replace('/', '\\')}\\Docs\\";
			public string[] AsVisual()
			{
				if (visualNames != null && visualNames.IsAlive)
					return (string[])visualNames.Target;
				string[] output = new string[Count];
				for (int i = 0; i < output.Length; i++)
				{
					output[i] = ToVisual(this[i].FullName);
				}
				visualNames = new WeakReference(output);
				return output;
			}
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
			string[] allFullPaths = ScriptHandler.AllDocuments.AsVisual();
			if (allFullPaths.Length > 0)
			{
				int oldIndex = Array.IndexOf(allFullPaths, scriptHandler.defaultStreamingAssetsDocumentPath);
				if (oldIndex < 0)
				{
					oldIndex = 0;
					scriptHandler.defaultStreamingAssetsDocumentPath = allFullPaths[0];
					EditorUtility.SetDirty(scriptHandler);
				}
				int newIndex = DirtyAuto.Popup(scriptHandler, new GUIContent("Starting Script"), oldIndex, allFullPaths);
				if (oldIndex != newIndex)
				{
					scriptHandler.defaultStreamingAssetsDocumentPath = allFullPaths[newIndex];
					EditorUtility.SetDirty(scriptHandler);
				}
				if (GUILayout.Button("Open File"))
					Process.Start(ScriptHandler.AllDocuments[newIndex].FullName);
			}
			else
			{
				EditorGUILayout.HelpBox("There is no .txt files in the " +
					"StreamingAssets folder found!", MessageType.Error);
			}
			InputActions(scriptHandler);
			if (scriptHandler.IsActive)
				EditorGUILayout.LabelField($"Current Line: {scriptHandler.documentWatcher.CurrentNode.PrimaryLine}");
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