namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting.Experimental;
	using B1NARY.UI;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
	using System.Collections;
	using System.Reflection;

	public delegate void SceneManagerEvent(string sceneName);
	public class SceneManager
	{
		public static string CurrentScene => UnitySceneManager.GetActiveScene().name;
		public static readonly IReadOnlyDictionary<string, Delegate> SceneDelegateCommands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)ChangeScene,
		};
		private const string reference = "B1NARY Transition Animation";

		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize(float fadeTime = 0.5f)
		{
			AddNonPersistentListener(LoadSpeakingObjects);
			ScriptHandler.Instance.InitializeNewScript();
			ScriptHandler.Instance.NextLine().FreeBlockPath();
			//ScriptHandler.Instance.NextLine();
			void LoadSpeakingObjects(string sceneName)
			{
				DialogueSystem.Instance.FadeIn(fadeTime);
			}
		}

		/// <summary>
		/// Changes the scene, a Boneless version of <see cref="FadeToNextScene(string, float)"/>
		/// </summary>
		/// <param name="sceneName">Name of the scene.</param>
		public static void ChangeScene(string sceneName)
		{
			FadeToNextScene(sceneName, 0, 1f).FreeBlockPath();
		}
		/*
		public static void ChangeScene(MonoBehaviour monoBehaviour, string sceneName)
		{
			
			monoBehaviour.StartCoroutine(Coroutine());
			IEnumerator Coroutine()
			{
				Task task = FadeToNextScene(sceneName, 0, 1f);
				while (task.IsCompleted == false)
					yield return new WaitForEndOfFrame();
			}
		}
		*/
		private static bool isFadingIn;
		public static Task FadeToNextScene(string sceneName, string transitionName, float fadeMultiplier = 1f)
		{
			var enumerator = Multiton<TransitionObject>.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.name != transitionName)
					continue;
				return FadeToNextScene(sceneName, enumerator.Current, fadeMultiplier);
			}
			throw new KeyNotFoundException($"{transitionName} as a transition object doesn't exist!");
		}
		public static Task FadeToNextScene(string sceneName, int transitionIndex, float fadeMultiplier = 1f)
		{
			var enumerator = Multiton<TransitionObject>.GetEnumerator();
			for (int i = 0; i < transitionIndex; i++)
				if (enumerator.MoveNext() == false)
					throw new IndexOutOfRangeException(transitionIndex.ToString());
			return FadeToNextScene(sceneName, enumerator.Current, fadeMultiplier);
		}
		public static async Task FadeToNextScene(string sceneName, float fadeMultiplier)
		{
			if (isFadingIn)
				throw new Exception();
			isFadingIn = true;
			PrepareSwitchScenes();
			Debug.Log("Fade In Stage");
			await TransitionManager.Instance.SetToOpaque(0, fadeMultiplier);
			Debug.Log("Loading Objects");
			SwitchScenes(sceneName);
			Debug.Log("Fade Out Stages");
			await TransitionManager.Instance.SetToTransparent(0, fadeMultiplier);
			Debug.Log("Finished Loading Stage");
			isFadingIn = false;
		}
		public static async Task FadeToNextScene(string sceneName, TransitionObject transitionObject, float fadeMultiplier = 1f)
		{
			// string debugPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/I did it.txt";
			// var stream = new StreamWriter(File.Create(debugPath)) { AutoFlush = true };
			if (isFadingIn)
				throw new Exception();
			isFadingIn = true;
			PrepareSwitchScenes();
			Debug.Log("Fade In Stage");
			//var coroutine = new CoroutineWrapper(TransitionCheck(true));
			//coroutine.ContinueWith(() => { Debug.Log("Loading Objects"); return SwitchScenes(sceneName); });
			await Multiton<TransitionObject>.First().SetToOpaque();
			Debug.Log("Loading Objects"); 
			SwitchScenes(sceneName);
			//coroutine.ContinueWith(() => Debug.Log("Fade Out Stages"));
			//coroutine.ContinueWith(TransitionCheck(false));
			//coroutine.ContinueWith(() => Debug.Log("Finished Loading Stage"));
			//coroutine.Start(ScriptHandler.Instance);
			//await transitionObject.SetToOpaque(fadeMultiplier);
			//await SwitchScenes(sceneName);
			//await Task.Run(() => SpinWait.SpinUntil(() => transitionObject.TransitionStatus == TransitionStatus.Transparent
			isFadingIn = false;
			//IEnumerator TransitionCheck(bool toOpaque)
			//{
			//	if (TransitionObject.Any())
			//		if (toOpaque)
			//			return TransitionObject.First().SetToOpaqueEnumerator(fadeMultiplier);
			//		else
			//			return TransitionObject.First().SetToTransparentEnumerator(fadeMultiplier);
			//	throw new MissingMemberException();
			//}
		}
		/// <summary>
		/// Invokes <see cref="SwitchingScenes"/> and clear it.
		/// </summary>
		private static void PrepareSwitchScenes()
		{
			if (PreppedSwitchScenes)
				throw new Exception();
			PreppedSwitchScenes = true;
			// Some events wants to constantly listen to scene switches, so they usually
			// - re-assign the same method. This allows them to have the ability to
			// - easily do so without having a 'buffer' or wait until it is finished
			// - emptying out the event.
			if (SwitchingScenes == null)
				return;
			Action[] events = SwitchingScenes.GetInvocationList().Cast<Action>().ToArray();
			SwitchingScenes = null;
			Array.ForEach(events, del => del.Invoke());
		}
		public static event Action SwitchingScenes;
		public static bool PreppedSwitchScenes { get; private set; } = false;
		

		/// <summary>
		/// Invokes <see cref="SwitchedScenes"/> and clear it. Also loads the scene
		/// right before/during scene switch.
		/// </summary>
		/// <param name="sceneName">Name of the scene to switch.</param>
		/// <returns> When the scene is finished loading. </returns>
		private static void SwitchScenes(string sceneName)
		{
			if (!PreppedSwitchScenes)
				throw new Exception();
			PreppedSwitchScenes = false;
			UnitySceneManager.LoadScene(sceneName);
			IEnumerable<SceneManagerEvent> allEvents = 
				new IEnumerable<SceneManagerEvent>[] { nonPersistentEvents, persistentEvents }
				.SelectMany(dict => dict);
			nonPersistentEvents.Clear();
			LoadingScreenAPI.LoadObjects(allEvents, sceneName);
		}

		#region Switched Scene Behaviour
		private static List<SceneManagerEvent> nonPersistentEvents = new List<SceneManagerEvent>(),
			persistentEvents = new List<SceneManagerEvent>();
		/// <summary>
		/// Stores and executes a method that is activated once, and not on
		/// concurrent events.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public static void AddNonPersistentListener(SceneManagerEvent sceneManagerEvent)
			=> nonPersistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Stores and executes a method that is activated every time.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public static void AddPersistentListener(SceneManagerEvent sceneManagerEvent)
			=> persistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Removes the 
		/// </summary>
		/// <param name="sceneManagerEvent">The scene manager event.</param>
		public static bool RemoveListener(SceneManagerEvent sceneManagerEvent)
		{
			if (nonPersistentEvents.Contains(sceneManagerEvent))
			{
				nonPersistentEvents.Remove(sceneManagerEvent);
				return true;
			}
			if (persistentEvents.Contains(sceneManagerEvent))
			{
				persistentEvents.Remove(sceneManagerEvent);
				return true;
			}
			return false;
		}
		#endregion
	}
}