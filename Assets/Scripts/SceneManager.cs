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
	/// An event that is invoked when <see cref="PersistentListenerGroup"/>
	/// is played. Roughly similar to <see cref="Action"/>
	/// </summary>
	public delegate void SwitchScenesDelegate();

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
		public static readonly IReadOnlyDictionary<string, Delegate> Commands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)ChangeSceneCommand,
		};
		[ForcePause]
		private static void ChangeSceneCommand(string newScene)
		{
			InstanceOrDefault.StartCoroutine(InstanceOrDefault.ChangeScene(newScene));
		}
		/// <summary> Gets the currently active/main scene. </summary>
		public static Scene ActiveScene => UnitySceneManager.GetActiveScene();

		/// <summary>
		/// A group of events that is invoked when the scene is starting to being 
		/// switched by various helper methods such as <see cref="ChangeScene(string)"/>
		/// </summary>
		public PersistentListenerGroup SwitchingScenes { get; private set; } = new PersistentListenerGroup();
		/// <summary>
		/// A group of events that is invoked when the scene is being switched
		/// by <see cref="UnitySceneManager.activeSceneChanged"/>.
		/// </summary>
		public PersistentListenerGroup SwitchedScenes { get; private set; } = new PersistentListenerGroup();
		/// <summary>
		/// If the current system is switching scenes. Automatically invoked by
		/// <see cref="SwitchingScenes"/> and <see cref="SwitchedScenes"/>.
		/// </summary>
		public bool IsSwitchingScenes { get; private set; } = false;

		protected override void SingletonAwake()
		{
			UnitySceneManager.activeSceneChanged += (scene1, scene2) => this.SwitchedScenes.Invoke();

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
			SwitchingScenes.Invoke();
			bool cannotPerformNext = true;
			SwitchedScenes.AddNonPersistentListener(() => cannotPerformNext = false);
			int oldIndex = ActiveScene.buildIndex;
			var async = UnitySceneManager.LoadSceneAsync(sceneName);
			if (async == null)
			{
				if (Application.isEditor)
				{
					Debug.LogException(new MissingReferenceException($"'{sceneName}' does not exist as a scene, fix this before production!"));
					ScriptHandler.Instance.ShouldPause = true;
				}
				else
					Utils.ForceCrash(ForcedCrashCategory.FatalError);
			}

			yield return new WaitUntil(() => async.isDone);
			//if (UnitySceneManager.GetActiveScene().buildIndex == oldIndex)
				

			do yield return new WaitForFixedUpdate();
			while (cannotPerformNext);
			ScriptHandler.Instance.ShouldPause = false;
			ScriptHandler.Instance.NextLine(); 
		}
		/// <summary>
		/// Initializes the <see cref="ScriptHandler"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		public void InitializeGame()
		{
			ScriptHandler.Instance.InitializeNewScript();
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
			IEnumerator transition = TransitionObject.First.SetToOpaqueEnumerator();
			while (transition.MoveNext())
				yield return transition.Current;
		}
	}

	/// <summary>
	/// A class that tracks both persistent and non-persistent listeners. Roughly
	/// similar to <see cref="UnityEngine.Events.UnityEvent"/>.
	/// </summary>
	public sealed class PersistentListenerGroup : IEnumerable<SwitchScenesDelegate>
	{
		/// <summary>
		/// A single delegate that invokes all methods in the array. No clearing
		/// needed, as long as <see cref="Invoke"/> is called.
		/// </summary>
		public Action<SwitchScenesDelegate[]> loadingAPIDelegate = actions =>
		{
			for (int i = 0; i < actions.Length; i++)
			{
				Debug.Log($"Loading method: {actions[i].Method.Name}");
				actions[i].Invoke();
			}
		};
		/// <summary>
		/// Invokes all <see cref="persistentListeners"/> and 
		/// <see cref="nonPersistentListeners"/>. Clears all 
		/// <see cref="nonPersistentListeners"/> afterwards.
		/// </summary>
		internal void Invoke()
		{
			SwitchScenesDelegate[] delegates =
				new List<SwitchScenesDelegate>[] { persistentListeners, nonPersistentListeners }
				.SelectMany(list => list).ToArray();
			nonPersistentListeners.Clear();
			loadingAPIDelegate.Invoke(delegates);
		}
		private readonly List<SwitchScenesDelegate> persistentListeners = new List<SwitchScenesDelegate>();
		private readonly List<SwitchScenesDelegate> nonPersistentListeners = new List<SwitchScenesDelegate>();
		/// <summary>
		/// All persistent listeners that stay after being added so it would be
		/// invoked multiple times.
		/// </summary>
		public IReadOnlyList<SwitchScenesDelegate> PersistentListeners => persistentListeners;
		/// <summary>
		/// All non-persistent listeners that dont stay after being invoked, so
		/// it is invoked once after being added.
		/// </summary>
		public IReadOnlyList<SwitchScenesDelegate> NonPersistentListeners => nonPersistentListeners;


		/// <summary>
		/// Adds a single persistent listener that listens to an event multiple
		/// times and persists.
		/// </summary>
		/// <param name="delegate"> The delegate to invoke multiple times. </param>
		/// <exception cref="InvalidOperationException"/>
		public void AddPersistentListener(SwitchScenesDelegate @delegate)
		{
			if (persistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			persistentListeners.Add(@delegate);
		}
		/// <summary>
		/// Removes a single persistent listener that lsitens to an event multiple
		/// times.
		/// </summary>
		/// <param name="delegate"> The delegate that is invoked multiple times. </param>
		public void RemovePersistentListener(SwitchScenesDelegate @delegate) =>
			persistentListeners.Remove(@delegate);
		/// <summary>
		/// Adds a non-persistent listener that gets destroyed after being invoked
		/// once.
		/// </summary>
		/// <param name="delegate"> The delegate that is invoked once. </param>
		/// <exception cref="InvalidOperationException"/>
		public void AddNonPersistentListener(SwitchScenesDelegate @delegate)
		{
			if (nonPersistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			nonPersistentListeners.Add(@delegate);
		}
		/// <summary>
		/// Removes a non-Persistent Listener.
		/// </summary>
		/// <param name="delegate">An void delegate with no arguments. </param>
		public void RemoveNonPersistentListener(SwitchScenesDelegate @delegate)
			=> nonPersistentListeners.Remove(@delegate);

		public IEnumerable<SwitchScenesDelegate> AsEnumerable()
		{
			for (int i = 0; i < persistentListeners.Count; i++)
				yield return persistentListeners[i];
			for (int i = 0; i < nonPersistentListeners.Count; i++)
				yield return nonPersistentListeners[i];
		}
		public IEnumerator<SwitchScenesDelegate> GetEnumerator() => AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => AsEnumerable().GetEnumerator();
	}
}