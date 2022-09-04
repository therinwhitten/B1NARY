namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting.Experimental;
	using B1NARY.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

	public delegate void SceneManagerEvent(string sceneName);
	public class SceneManager
	{
		public static string CurrentScene => UnitySceneManager.GetActiveScene().name;
		public static readonly IReadOnlyDictionary<string, Delegate> SceneDelegateCommands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)ChangeScene,
		};
		/// <summary>
		/// Data
		/// </summary>
		public static SwitchedScenes SwitchedScenes { get; private set; } = new SwitchedScenes();
		public static SwitchingScenes SwitchingScenes { get; private set; } = new SwitchingScenes();

		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize(float fadeTime = 0.5f)
		{
			SwitchedScenes.AddNonPersistentListener(LoadSpeakingObjects);
			ScriptHandler.Instance.InitializeNewScript();
			ScriptHandler.Instance.NextLine().FreeBlockPath();
			//ScriptHandler.Instance.NextLine();
			void LoadSpeakingObjects(string sceneName)
			{
				DialogueSystem.Instance.FadeIn(fadeTime);
			}
		}

		/// <summary>
		/// Changes the scene, a Boneless version of <see cref="FadeToNextScene(string, TransitionObject, float)"/>
		/// </summary>
		/// <param name="sceneName">Name of the scene.</param>
		public static void ChangeScene(string sceneName)
		{
			FadeToNextScene(sceneName, 0, 1f).FreeBlockPath();
		}
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
			=> FadeToNextScene(sceneName, Multiton<TransitionObject>.GetViaIndex(transitionIndex), fadeMultiplier);
		public static Task FadeToNextScene(string sceneName, float fadeMultiplier = 1f)
			=> FadeToNextScene(sceneName, Multiton<TransitionObject>.First(), fadeMultiplier);
		/// <summary>
		///		<para>
		///			Fades to next scene.
		///		</para>
		///		<br>
		///			This executes a list of commands:
		///			<list type="number">
		///				<item>use <see cref="SwitchingScenes"/> to execute all events tied to that.</item>
		///				<item>
		///					use <paramref name="transitionObject"/> to fade 
		///					in/to opaque, if <see langword="null"/>, use 2000 ms 
		///					to simaulate it
		///				</item>
		///				<item>
		///					actually switch scenes using 
		///					<see cref="SwitchedScenes.SwitchScenes(string)"/>,
		///					which contains a scene change command inide.
		///				</item>
		///			</list>
		///		</br>
		///	</summary>
		/// <param name="sceneName">Name of the scene to switch to.</param>
		/// <param name="transitionObject">The transition slide data. </param>
		/// <param name="fadeMultiplier">The fade/speed animation multiplier.</param>
		public static async Task FadeToNextScene(string sceneName, 
			TransitionObject transitionObject, float fadeMultiplier = 1f)
		{
			SwitchingScenes.PrepareSwitchingScenes();
			if (transitionObject == null)
			{
				Debug.Log($"{nameof(transitionObject)} is null, waiting for 2 sec.");
				await Task.Delay(2000 / (int)fadeMultiplier);
			}
			else
				await transitionObject.SetToOpaque(fadeMultiplier);
			SwitchedScenes.SwitchScenes(sceneName);
		}
	}

	public class SwitchingScenes
	{
		internal SwitchingScenes()
		{

		}
		internal void PrepareSwitchingScenes()
		{
			// Some events wants to constantly listen to scene switches, so they usually
			// - re-assign the same method. This allows them to have the ability to
			// - easily do so without having a 'buffer' or wait until it is finished
			// - emptying out the event.
			IEnumerable<Action> allEvents = new List<Action>[] { nonPersistentEvents, persistentEvents }
				.SelectMany(list => list);
			nonPersistentEvents.Clear();
			// Invokes and compiles at compile time, not runtime.
			allEvents.Select(action => { action.Invoke(); return action; });
		}

		private List<Action> nonPersistentEvents = new List<Action>(),
			persistentEvents = new List<Action>();
		/// <summary>
		/// Stores and executes a method that is activated once, and not on
		/// concurrent events.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public void AddNonPersistentListener(Action sceneManagerEvent)
			=> nonPersistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Stores and executes a method that is activated every time the scene
		/// is changed.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public void AddPersistentListener(Action sceneManagerEvent)
			=> persistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Removes the 
		/// </summary>
		/// <param name="sceneManagerEvent">The scene manager event.</param>
		public bool RemoveListener(Action sceneManagerEvent)
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
	}
	public class SwitchedScenes
	{ 
		internal SwitchedScenes()
		{

		}


		/// <summary>
		/// Invokes <see cref="SwitchedScenes"/> and clear it. Also loads the scene
		/// right before/during scene switch.
		/// </summary>
		/// <param name="sceneName">Name of the scene to switch.</param>
		/// <returns> When the scene is finished loading. </returns>
		internal void SwitchScenes(string sceneName)
		{
			UnitySceneManager.LoadScene(sceneName);
			IEnumerable<SceneManagerEvent> allEvents =
				new IEnumerable<SceneManagerEvent>[] { nonPersistentEvents, persistentEvents }
				.SelectMany(dict => dict);
			nonPersistentEvents.Clear();
			LoadingScreenAPI.LoadObjects(allEvents, sceneName);
		}

		private List<SceneManagerEvent> nonPersistentEvents = new List<SceneManagerEvent>(),
			persistentEvents = new List<SceneManagerEvent>();
		/// <summary>
		/// Stores and executes a method that is activated once, and not on
		/// concurrent events.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public void AddNonPersistentListener(SceneManagerEvent sceneManagerEvent)
			=> nonPersistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Stores and executes a method that is activated every time the scene
		/// is changed.
		/// </summary>
		/// <param name="sceneManagerEvent">The method to invoke.</param>
		public void AddPersistentListener(SceneManagerEvent sceneManagerEvent)
			=> persistentEvents.Add(sceneManagerEvent);
		/// <summary>
		/// Removes the 
		/// </summary>
		/// <param name="sceneManagerEvent">The scene manager event.</param>
		public bool RemoveListener(SceneManagerEvent sceneManagerEvent)
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
	}
}