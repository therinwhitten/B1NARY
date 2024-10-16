﻿namespace B1NARY.Scripting
{
	using B1NARY.Audio;
	using B1NARY.CharacterManagement;
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using OVSSerializer.IO;
	using B1NARY.Steamworks;
	using B1NARY.UI;
	using B1NARY.UI.Colors;
	using B1NARY.UI.Globalization;
	using HDConsole;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;
	using CharacterManager = CharacterManagement.CharacterManager;

	[AddComponentMenu("B1NARY/Script Handler")]
	public sealed class ScriptHandler : DesignPatterns.Singleton<ScriptHandler>
	{
		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("script", (args) =>
			{
				ScriptHandler handler = Instance;
				handler.NewDocument(args[0]);
				Debug.Log($"Next document to line: {handler.NextLine()}");

			}) { description = "Switches the script via name." },

			new HDCommand("scripts_all", (args) =>
			{
				StringBuilder allScripts = new("<b><size=135%>Default/Core:</size></b>\n");
				for (int i = 0; i < DocumentExplorer.CoreDocuments.Count; i++)
				{
					Document document = DocumentExplorer.CoreDocuments[i];
					allScripts.AppendLine($"\t{document.FullPath.FullPath}");
				}
				using var enumerator = DocumentExplorer.LanguagedDocuments.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string language = enumerator.Current.Key;
					IList<Document> documents = enumerator.Current.Value;
					allScripts.AppendLine($"<b><size=135%>{language}:</size></b>");
					for (int i = 0; i < documents.Count; i++)
						allScripts.AppendLine($"\t{documents[i].FullPath.FullPath}");
				}
				HDConsole.WriteLine(allScripts.ToString());
			}) { description = "Prints of all detected documents to console." },
		};


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
						["usegameobject"] = (Action<string, string>)((gameObjectName, pauseGame) => UseGameObject(gameObjectName, bool.Parse(pauseGame))),
						["stoponline"] = (Action)(StopOnLine),
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
						// Invokes script commands through console; dont need commas so extra args doesn't do much.
						["invokeconsolecommand"] = (Action<string>)((arg1) => HDConsole.InvokeThoughConsole(arg1)),
						// Keeping these here for the hell of it lol
						["invokeconsolecommand"] = (Action<string, string>)((arg1, arg2) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2))),
						["invokeconsolecommand"] = (Action<string, string, string>)((arg1, arg2, arg3) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3))),
						["invokeconsolecommand"] = (Action<string, string, string, string>)((arg1, arg2, arg3, arg4) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15))),
						["invokeconsolecommand"] = (Action<string, string, string, string, string, string, string, string, string, string, string, string, string, string, string, string>)((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) => HDConsole.InvokeThoughConsole(CommandJoin(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16))),
					},
					DialogueSystem.Commands,
					AudioController.Commands,
					SceneManager.Commands,
					CharacterManager.Commands,
					TransitionManager.Commands,
					ColorFormat.Commands,
					FollowCube.Commands,
					ButtonInvoker.Commands,
					VoiceActorHandler.Commands,
					AchievementManager.Commands,
				}.SelectMany<CommandArray, OverloadableCommand>(commands => commands));
			static string CommandJoin(params string[] args) => string.Join(", ", args);
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
			GameObject @object = Marker.GetMarkers(objectName).SingleOrDefault().gameObject;
			if (@object == null)
				throw new MissingMemberException($"Gameobject '{objectName}' is not found");
			@object.SetActive(true);
			Instance.pauser.AddBlocker(Instance);
			Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => !@object.activeSelf);
				Instance.pauser.RemoveBlocker(Instance);
				Instance.NextLine();
			}
		}
		[ForcePause]
		internal static void StopOnLine()
		{

		}
		internal static void UseGameObject(string objectName, bool pauseGame)
		{
			if (pauseGame)
			{
				UseGameObject(objectName);
				return;
			}
			GameObject @object = Marker.GetMarkers(objectName).SingleOrDefault().gameObject;
			if (@object == null)
				throw new MissingMemberException($"Gameobject '{objectName}' is not found");
			@object.SetActive(true);
		}
		public static DocumentExplorer DocumentExplorer { get; } = new DocumentExplorer();



		public Pauser pauser = new();
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

		protected override void SingletonAwake()
		{
			DontDestroyOnLoad(transform.root);
			config.NormalLine += SayLine;
			config.AttributeListeners += ChangeExpression;
			config.EntryListeners += ChangeCharacter;
			PlayerConfig.Instance.language.ValueChanged += UpdateDocumentViaLanguage;
			for (int i = 0; i < nextLineButtons.Length; i++)
				playerInput.actions.FindAction(nextLineButtons[i], true).performed += NextLineKey;
		}
		protected override void OnSingletonDestroy()
		{
			PlayerConfig.Instance.language.ValueChanged -= UpdateDocumentViaLanguage;
			for (int i = 0; i < nextLineButtons.Length; i++)
				playerInput.actions.FindAction(nextLineButtons[i], false).performed -= NextLineKey;
		}
		private void NextLineKey(InputAction.CallbackContext context) => NextLine();
		private void UpdateDocumentViaLanguage(string newLanguage)
		{
			if (documentWatcher == null || documentWatcher.EndOfDocument)
				return;
			int currentIndex = documentWatcher.CurrentNode.GlobalIndex;
			Document newDocument = new(document.ReadFile);
			newDocument = newDocument.GetWithLanguage(newLanguage);
			OSFile file = newDocument.FullPath;
			if (!file.Exists)
			{
				Debug.LogWarning($"File '{file.FullPath}' doesn't exist, using core path instead.");
				file = newDocument.GetWithoutLanguage().FullPath;
			}
			document = new ScriptDocument(config, file);
			documentWatcher = document.StartAtLine(currentIndex - 1);
			Pauser pausedPauser = this.pauser;
			using (this.pauser = new Pauser())
				NextLine(true);
			this.pauser = pausedPauser;
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
			CharacterManager.Instance.ChangeActiveCharacterViaCharacterName(newCharacter);
		}


		public void NewDocument(int index = 0)
		{
			NewDocument(defaultStreamingAssetsDocumentPath, index);
		}
		public void NewDocument(string streamingAssetsDocument, int index = 0)
		{
			document = new ScriptDocument(config, DocumentExplorer.GetDocumentFromVisual(streamingAssetsDocument).FullPath);
			documentWatcher = document.StartAtLine(index);
		}

		public ScriptNode NextLine(bool forceContinue = false)
		{
			if (pauser.Blocking)
			{
				Debug.Log("Pausing is enabled.");
				return documentWatcher.CurrentNode;
			}
			if (document is null || documentWatcher is null)
			{
				return null;
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
			string[] allVisualPaths = ScriptHandler.DocumentExplorer.CoreDocuments.Select(doc => doc.VisualPath).ToArray();
			if (allVisualPaths.Length > 0)
			{
				int oldIndex = Array.IndexOf(allVisualPaths, scriptHandler.defaultStreamingAssetsDocumentPath);
				if (oldIndex < 0)
				{
					oldIndex = 0;
					scriptHandler.defaultStreamingAssetsDocumentPath = allVisualPaths[0];
					EditorUtility.SetDirty(scriptHandler);
				}
				int newIndex = DirtyAuto.Popup(scriptHandler, new GUIContent("Starting Script"), oldIndex, allVisualPaths);
				if (oldIndex != newIndex)
				{
					scriptHandler.defaultStreamingAssetsDocumentPath = allVisualPaths[newIndex];
					EditorUtility.SetDirty(scriptHandler);
				}
				if (GUILayout.Button("Open File"))
					Process.Start(ScriptHandler.DocumentExplorer.CoreDocuments[newIndex].FullPath.FullPath);
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
						textEditorRect = new(fullRect) { xMax = fullRect.xMax / 5 * 4 },
						deleteButtonRect = new(fullRect) { xMin = textEditorRect.xMax + 2 };
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