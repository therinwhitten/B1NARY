namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting.Experimental;
	using B1NARY.UI;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

	public delegate void SceneManagerEvent(string sceneName);
	public delegate void SwitchScenesDelegate();
	public static class SceneManager
	{
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)(str => ChangeScene(str).FreeBlockPath()),
		};
		public static Lazy<Scene[]> activeScenes = new Lazy<Scene[]>(() =>
		{
			var scenes = new List<Scene>();
			for (int i = 0; i < UnitySceneManager.sceneCount; i++)
			{
				try { scenes.Add(UnitySceneManager.GetSceneByBuildIndex(i)); }
				catch { continue; }
			}
			return scenes.ToArray();
		});
		public static Scene ActiveScene => UnitySceneManager.GetActiveScene();

		public static PersistentListenerGroup SwitchingScenes { get; private set; } = new PersistentListenerGroup();
		public static PersistentListenerGroup SwitchedScenes { get; private set; } = new PersistentListenerGroup();

		public static async Task ChangeScene(string sceneName)
		{
			await FadeScene();
			SwitchingScenes.Invoke();
			UnitySceneManager.LoadScene(sceneName);
		}
		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize()//float fadeTime = 0.5f)
		{
			//SwitchedScenes.AddNonPersistentListener(LoadSpeakingObjects);
			ScriptHandler.Instance.InitializeNewScript();
			ScriptHandler.Instance.NextLine().FreeBlockPath();
			//ScriptHandler.Instance.NextLine();
			//void LoadSpeakingObjects()
			//{
			//	DialogueSystem.Instance.FadeIn(fadeTime);
			//}
		}

		private static async Task FadeScene()
		{
			if (!TransitionObject.Any)
			{
				Debug.LogWarning($"Was called to fade scene, but there are no {nameof(TransitionObject)}s to transition with!\n{nameof(SceneManager)}");
				return;
			}
			TransitionObject transition = TransitionObject.First;
			await transition.SetToOpaque();
		}
		/*

		

		

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
		*/
	}

	public sealed class PersistentListenerGroup : IEnumerable<SwitchScenesDelegate>
	{
		public Action<SwitchScenesDelegate[]> loadingAPIDelegate = actions =>
		{
			for (int i = 0; i < actions.Length; i++)
			{
				Debug.Log($"Loading method: {actions[i].Method.Name}");
				actions[i].Invoke();
			}
		};
		internal void Invoke()
		{
			SwitchScenesDelegate[] delegates =
				new List<SwitchScenesDelegate>[] { persistentListeners, nonPersistentListeners }
				.SelectMany(list => list).ToArray();
			nonPersistentListeners.Clear();
			loadingAPIDelegate.Invoke(delegates);
		}
		private readonly List<SwitchScenesDelegate> persistentListeners = new List<SwitchScenesDelegate>(),
			nonPersistentListeners = new List<SwitchScenesDelegate>();


		public void AddPersistentListener(SwitchScenesDelegate @delegate)
		{
			if (persistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			persistentListeners.Add(@delegate);
		}
		public void RemovePersistentListener(SwitchScenesDelegate @delegate) =>
			persistentListeners.Remove(@delegate);
		public void AddNonPersistentListener(SwitchScenesDelegate @delegate)
		{
			if (nonPersistentListeners.Contains(@delegate))
				throw new InvalidOperationException();
			nonPersistentListeners.Add(@delegate);
		}
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