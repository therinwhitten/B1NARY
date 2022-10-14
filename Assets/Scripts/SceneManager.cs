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
	public class SceneManager : Singleton<SceneManager>
	{
		public static readonly IReadOnlyDictionary<string, Delegate> Commands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)(str => InstanceOrDefault.ChangeScene(str).Wait()),
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

		public PersistentListenerGroup SwitchingScenes { get; private set; } = new PersistentListenerGroup();
		public PersistentListenerGroup SwitchedScenes { get; private set; } = new PersistentListenerGroup();

		protected override void SingletonAwake()
		{
			UnitySceneManager.activeSceneChanged += (scene1, scene2) => SwitchedScenes.Invoke();
		}

		public async Task ChangeScene(string sceneName)
		{
			await FadeScene();
			SwitchingScenes.Invoke();
			UnitySceneManager.LoadScene(sceneName);
			StartCoroutine(AfterLoadTask());
			//SwitchedScenes.Invoke();
			IEnumerator AfterLoadTask()
			{
				yield return new WaitForFixedUpdate();
				ScriptHandler.Instance.NextLine().Wait();
			}
		}
		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize()
		{
			ScriptHandler.Instance.InitializeNewScript();
			ScriptHandler.Instance.NextLine().FreeBlockPath();
		}

		private async Task FadeScene()
		{
			if (!TransitionObject.Any)
			{
				Debug.LogWarning($"Was called to fade scene, but there are no {nameof(TransitionObject)}s to transition with!\n{nameof(SceneManager)}");
				return;
			}
			TransitionObject transition = TransitionObject.First;
			await transition.SetToOpaque();
		}
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