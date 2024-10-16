﻿namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using B1NARY.UI.Colors;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Diagnostics;
	using UnityEngine.SceneManagement;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

	/// <summary>
	/// an event or method that is invoked when the scene is transitioned.
	/// Similar to <see cref="Action{string}"/>
	/// </summary>
	/// <param name="sceneName"> The scene name. </param>
	public delegate void SceneManagerEvent(string sceneName);

	/// <summary>
	/// An instance inside the unity scene tied as a DoNotDestroy <see cref="GameObject"/>
	/// that keeps track of what scene it is in and invokes <see cref="SwitchedScenes"/>
	/// and <see cref="SwitchingScenes"/>. Also tells the developer which events
	/// are added in.
	/// </summary>
	public class SceneManager : Singleton<SceneManager>
	{
		/// <summary>
		/// All script-based commands for <see cref="ScriptDocument"/>
		/// </summary>
		public static readonly CommandArray Commands = new()
		{
			["changescene"] = (Action<string>)ChangeSceneCommand,
			["returntomainmenu"] = (Action)ReturnToMainMenuCommand,
		};
		[ForcePause]
		private static void ChangeSceneCommand(string newScene)
		{
			InstanceOrDefault.StartCoroutine(InstanceOrDefault.ChangeScene(newScene, true));
		}
		[ForcePause]
		internal static void ReturnToMainMenuCommand()
		{
			CoroutineWrapper wrapper = new(InstanceOrDefault, InstanceOrDefault.ReturnToMainMenu());
			wrapper.AfterActions += (mono) =>
			{
				if (!ScriptHandler.TryGetInstance(out var handler))
					return;
				handler.document = null;
				handler.documentWatcher = null;
			};
			wrapper.Start();
		}
		/// <summary> Gets the currently active/main scene. </summary>
		public static Scene ActiveScene => UnitySceneManager.GetActiveScene();

		/// <summary>
		/// Activates a scene by name.
		/// </summary>
		/// <param name="sceneName"> The name of the scene to activate. </param>
		public void ActivateSceneByName(string sceneName)
		{
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

			if (scene.IsValid())
			{
				UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
			}
			else
			{
				Debug.LogError($"Scene with name '{sceneName}' is not valid or not found.");
			}
		}


		/// <summary>
		/// A group of events that is invoked when the scene is starting to being 
		/// switched by various helper methods such as <see cref="ChangeScene(string)"/>
		/// </summary>
		public ListenerGroup SwitchingScenes { get; private set; } = new ListenerGroup();
		/// <summary>
		/// A group of events that is invoked when the scene is being switched
		/// by <see cref="UnitySceneManager.activeSceneChanged"/>.
		/// </summary>
		public ListenerGroup SwitchedScenes { get; private set; } = new ListenerGroup();
		/// <summary>
		/// If the current system is switching scenes. Automatically invoked by
		/// <see cref="SwitchingScenes"/> and <see cref="SwitchedScenes"/>.
		/// </summary>
		public bool IsSwitchingScenes { get; private set; } = false;
		public int MainMenuSceneIndex = 1;

		protected override void SingletonAwake()
		{
			UnitySceneManager.activeSceneChanged += (scene1, scene2) => this.SwitchedScenes.InvokeAll();

			this.SwitchingScenes.AddPersistentListener(SwitchingScenes);
			this.SwitchedScenes.AddPersistentListener(SwitchedScenes);

			void SwitchingScenes() => IsSwitchingScenes = true;
			void SwitchedScenes() => IsSwitchingScenes = false;
		}

		public IEnumerator ChangeScene(int sceneIndex)
		{
			Scene scene = UnitySceneManager.GetSceneByBuildIndex(sceneIndex);
			if (!scene.IsValid())
				throw new ArgumentNullException($"build index of '{sceneIndex}'"
					+ " does not lead to anything!");
			return ChangeScene(scene.name, true);
		}
		/// <summary>
		/// A coroutine that changes the scene to a new scene via the name.
		/// Waits for transitions from <see cref="FadeScene"/> and invokes 
		/// <see cref="ScriptHandler.NextLine"/> to continue the document.
		/// </summary>
		/// <param name="sceneName"> The scene to transition to. </param>
		/// <returns> The Coroutine. </returns>
		public IEnumerator ChangeScene(string sceneName, bool handleScripts)
		{
			if (handleScripts)
				ScriptHandler.Instance.pauser.AddBlocker(this);
			IEnumerator fadeEnum = FadeScene();
			while (fadeEnum.MoveNext())
				yield return fadeEnum.Current;

			IsSwitchingScenes = true;
			SwitchingScenes.InvokeAll();
			bool cannotPerformNext = true;
			SwitchedScenes.AddNonPersistentListener(() => cannotPerformNext = false);
			int oldIndex = ActiveScene.buildIndex;
			var async = UnitySceneManager.LoadSceneAsync(sceneName);
			if (async == null)
			{
				if (Application.isEditor)
				{
					Debug.LogException(new MissingReferenceException($"'{sceneName}' does not exist as a scene, fix this before production! IT WILL BREAK YOUR SHIT!"));
					ScriptHandler.Instance.pauser.Break();
				}
				else
					Utils.ForceCrash(ForcedCrashCategory.FatalError);
			}

			yield return new WaitUntil(() => async.isDone);
			do yield return new WaitForFixedUpdate();
			while (cannotPerformNext);

			if (handleScripts)
			{
				ScriptHandler.Instance.pauser.RemoveBlocker(this);
				ScriptHandler.Instance.NextLine();
			}
		}
		public IEnumerator ReturnToMainMenu()
		{
			ScriptHandler.Instance.pauser.AddBlocker(this);
			IEnumerator fadeEnum = FadeScene();
			while (fadeEnum.MoveNext())
				yield return fadeEnum.Current;

			IsSwitchingScenes = true;
			SwitchingScenes.InvokeAll();
			var async = UnitySceneManager.LoadSceneAsync(MainMenuSceneIndex);
			if (async == null)
			{
#if UNITY_EDITOR
				Debug.LogException(new MissingReferenceException($"'{MainMenuSceneIndex}' does not exist as a scene index, fix this before production! IT WILL BREAK YOUR SHIT!"));
				UnityEditor.EditorApplication.ExitPlaymode();
#else
				Utils.ForceCrash(ForcedCrashCategory.FatalError);
#endif
			}
			ScriptHandler.Instance.pauser.RemoveBlocker(this);
			ScriptHandler.Instance.Clear();
			AudioController.Instance.RemoveAllSounds();
			ColorFormat.SetFormat(ColorFormat.DefaultFormat, false);
		}
		/// <summary>
		/// Initializes the <see cref="ScriptHandler"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		public void InitializeGame()
		{
			SaveSlot.ActiveSlot =new SaveSlot();
			ScriptHandler.Instance.NewDocument();
			ScriptHandler.Instance.NextLine();
		}
		/// <summary>
		/// Initializes the <see cref="ScriptHandler"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		public void InitializeScript(string docPath)
		{
			SaveSlot.ActiveSlot = new SaveSlot();
			ScriptHandler.Instance.NewDocument(docPath);
			ScriptHandler.Instance.NextLine();
		}

		/// <summary>
		/// A Coroutine that either transitions instantly if there are no
		/// <see cref="TransitionObject"/>s in the scene, or waits for the first
		/// object added to fully complete to <see cref="TransitionStatus.Transparent"/>.
		/// </summary>
		/// <returns> The Coroutine to track. </returns>
		private IEnumerator FadeScene()
		{
			if (!TransitionObject.Any)
			{
				Debug.LogWarning($"Was called to fade scene, but there are no {nameof(TransitionObject)}s to transition with!\n{nameof(SceneManager)}");
				yield break;
			}
			IEnumerator transition = TransitionObject.Youngest.SetToOpaqueEnumerator();
			while (transition.MoveNext())
				yield return transition.Current;
		}

        internal static AsyncOperation LoadSceneAsync(string sceneName1, LoadSceneMode additive)
        {
            throw new NotImplementedException();
        }

        internal static void SetActiveScene(Scene scene2)
        {
            throw new NotImplementedException();
        }
    }

	
}
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using UnityEditor.UI;
	using System;

	[CustomEditor(typeof(SceneManager))]
	public class SceneManagerEditor : Editor
	{
		private SceneManager sceneManager;
		private void Awake() => sceneManager = (SceneManager)target;

		public override void OnInspectorGUI()
		{
			sceneManager.MainMenuSceneIndex = Mathf.Clamp(DirtyAuto.Field(sceneManager, new GUIContent("Main Menu Scene Index"), sceneManager.MainMenuSceneIndex), 0, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings - 1);
			EditorGUILayout.LabelField($"Active Scene: {SceneManager.ActiveScene.name}, Index: {SceneManager.ActiveScene.buildIndex}");
			EditorGUILayout.LabelField($"Currently Switching Scenes: {sceneManager.IsSwitchingScenes}");
			EditorGUILayout.Separator();
			DisplayTab("Before Switching Scenes", sceneManager.SwitchingScenes);
			DisplayTab("After Switching Scenes", sceneManager.SwitchedScenes);
		}

		/// <summary>
		/// Displays a tab's persistent and non-persistent listeners.
		/// </summary>
		/// <param name="header"></param>
		/// <param name="group"></param>
		/// <returns> The taken space in the rect. </returns>
		private static void DisplayTab(string header, ListenerGroup group)
		{
			Rect masterHeaderRect = GUILayoutUtility.GetRect(Screen.width / 4 * 3, 22f);
			masterHeaderRect = EditorGUI.IndentedRect(masterHeaderRect);
			EditorGUI.LabelField(masterHeaderRect, header, EditorStyles.largeLabel);
			EditorGUI.indentLevel++;
			DisplaySubCategory("Persistent Listeners", group.PersistentListeners);
			DisplaySubCategory("Non-Persistent Listeners", group.NonPersistentListeners);
			EditorGUI.indentLevel--;

			static void DisplaySubCategory(string subHeader, IReadOnlyList<Action> delegates)
			{
				EditorGUILayout.LabelField(subHeader, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				for (int i = 0; i < delegates.Count; i++)
					EditorGUILayout.LabelField($"{i + 1}. '{delegates[i].Method.Name}' from class '{delegates[i].Method.DeclaringType}'");
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif