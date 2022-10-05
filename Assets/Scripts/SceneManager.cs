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
	using UnityEngine.SceneManagement;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

	public delegate void SwitchScenesDelegate();

	public sealed class SceneManager : Singleton<SceneManager>
	{
		public static readonly IReadOnlyDictionary<string, Delegate> SceneDelegateCommands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)(str => _ = InstanceOrDefault.ChangeScene(str)),
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

		private void Awake()
		{
			UnitySceneManager.sceneLoaded += (old, @new) => SwitchedScenes.Invoke();
		}
		public async Task ChangeScene(string sceneName)
		{
			SwitchingScenes.AddNonPersistentListener(FadeScene);
			await Task.Run(SwitchingScenes.Invoke);
			UnitySceneManager.LoadScene(sceneName);
		}
		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current scriptName, or the first scriptName to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize()//float fadeTime = 0.5f)
		{
			ScriptHandler.Instance.InitializeNewScript();
			ScriptHandler.Instance.NextLine().FreeBlockPath();
		}

		private static void FadeScene()
		{
			TransitionObject transition = Multiton<TransitionObject>.First();
			transition.SetToOpaque().Wait();
		}
	}
	public sealed class PersistentListenerGroup
	{
		public static PersistentListenerGroup operator +(PersistentListenerGroup setter, SwitchScenesDelegate persistentListener)
		{
			setter.AddPersistentListener(persistentListener);
			return setter;
		}
		public static PersistentListenerGroup operator -(PersistentListenerGroup setter, SwitchScenesDelegate persistentListener)
		{
			setter.RemovePersistentListener(persistentListener);
			return setter;
		}

		public Action<SwitchScenesDelegate[]> loadingAPIDelegate = actions =>
		{
			for (int i = 0; i < actions.Length; i++)
			{
				Debug.Log($"Loading method: '{actions[i].Method.Name}' from class '{actions[i].Method.DeclaringType.Name}'\nScene Manager");
				try
				{
					actions[i].Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
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
		public bool ContainsPersistentListener(SwitchScenesDelegate @delegate)
			=> persistentListeners.Contains(@delegate);
		public bool ContainsNonPersistentListener(SwitchScenesDelegate @delegate)
			=> nonPersistentListeners.Contains(@delegate);
		public void RemoveListener(SwitchScenesDelegate @delegate)
		{
			if (ContainsNonPersistentListener(@delegate))
				RemoveNonPersistentListener(@delegate);
			else
				RemovePersistentListener(@delegate);
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
	}
}