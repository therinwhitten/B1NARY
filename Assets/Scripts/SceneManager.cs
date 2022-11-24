namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
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
		public static readonly CommandArray Commands = new CommandArray()
		{
			["changescene"] = (Action<string>)ChangeSceneCommand,//["changescene"] = (Action<string>)ChangeSceneCommand,
			["returntomainmenu"] = (Action)ReturnToMainMenuCommand,
		};
		[ForcePause]
		private static void ChangeSceneCommand(string newScene)
		{
			InstanceOrDefault.StartCoroutine(InstanceOrDefault.ChangeScene(newScene));
		}
		[ForcePause]
		internal static void ReturnToMainMenuCommand()
		{
			InstanceOrDefault.StartCoroutine(InstanceOrDefault.ReturnToMainMenu());
		}
		/// <summary> Gets the currently active/main scene. </summary>
		public static Scene ActiveScene => UnitySceneManager.GetActiveScene();

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

		/// <summary>
		/// A coroutine that changes the scene to a new scene via the name.
		/// Waits for transitions from <see cref="FadeScene"/> and invokes 
		/// <see cref="ScriptHandler.NextLine"/> to continue the document.
		/// </summary>
		/// <param name="sceneName"> The scene to transition to. </param>
		/// <returns> The Coroutine. </returns>
		public IEnumerator ChangeScene(string sceneName)
		{
			ScriptHandler.Instance.ShouldPause = true;
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
					ScriptHandler.Instance.ShouldPause = true;
				}
				else
					Utils.ForceCrash(ForcedCrashCategory.FatalError);
			}

			yield return new WaitUntil(() => async.isDone);
				

			do yield return new WaitForFixedUpdate();
			while (cannotPerformNext);
			ScriptHandler.Instance.ShouldPause = false;
			ScriptHandler.Instance.NextLine(); 
		}
		public IEnumerator ReturnToMainMenu()
		{
			ScriptHandler.Instance.ShouldPause = true;
			IEnumerator fadeEnum = FadeScene();
			while (fadeEnum.MoveNext())
				yield return fadeEnum.Current;

			IsSwitchingScenes = true;
			SwitchingScenes.InvokeAll();
			var async = UnitySceneManager.LoadSceneAsync(MainMenuSceneIndex);
			if (async == null)
			{
				if (Application.isEditor)
				{
					Debug.LogException(new MissingReferenceException($"'{MainMenuSceneIndex}' does not exist as a scene index, fix this before production! IT WILL BREAK YOUR SHIT!"));
					ScriptHandler.Instance.ShouldPause = true;
				}
				else
					Utils.ForceCrash(ForcedCrashCategory.FatalError);
			}
			ScriptHandler.Instance.ShouldPause = false;
			ScriptHandler.Instance.Clear();
		}
		/// <summary>
		/// Initializes the <see cref="ScriptHandler"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		public void InitializeGame()
		{
			ScriptHandler.Instance.InitializeNewScript();
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
			IEnumerator transition = TransitionObject.First.SetToOpaqueEnumerator();
			while (transition.MoveNext())
				yield return transition.Current;
		}
	}

	
}