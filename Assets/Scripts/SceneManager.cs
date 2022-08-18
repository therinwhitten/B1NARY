﻿namespace B1NARY
{
	using B1NARY.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

	public class SceneManager
	{
		public static string CurrentScene => UnitySceneManager.GetActiveScene().name;
		public static readonly IReadOnlyDictionary<string, Delegate> SceneDelegateCommands = new Dictionary<string, Delegate>()
		{
			["changescene"] = (Action<string>)(str => FadeSceneTask = FadeToNextScene(str, 1f)),
		};
		private const string reference = "B1NARY Transition Animation";

		/// <summary>
		/// Initialized the <see cref="ScriptParser"/>, the system expects the
		/// current script, or the first script to switch scenes on the first command.
		/// </summary>
		/// <param name="fadeTime">The fade time to take.</param>
		public static void Initialize(float fadeTime = 0.5f)
		{
			SwitchedScenes += LoadSpeakingObjects;
			ScriptParser.Instance.Initialize();
			void LoadSpeakingObjects(string sceneName)
			{
				B1NARYConsole.Log(nameof(SceneManager), sceneName, "Loading objects");
				DialogueSystem.Instance.FadeIn(fadeTime);
			}
		}

		private static Task FadeSceneTask = Task.CompletedTask;
		public static async Task FadeToNextScene(string sceneName, float fadeMultiplier = 1f)
		{
			if (!FadeSceneTask.IsCompleted)
				throw new Exception();
			PrepareSwitchScenes();
			Debug.Log("Fade In");
			await TransitionHandler.Instance.FadeIn(0, fadeMultiplier);
			Debug.Log("Scene Event");
			await SwitchScenes(sceneName);
			Debug.Log("Fade Out");
			await TransitionHandler.Instance.FadeOut(0, fadeMultiplier);
			Debug.Log("Switched scenes succ");
		}
		/// <summary>
		/// Invokes <see cref="SwitchingScenes"/> and clear it.
		/// </summary>
		private static void PrepareSwitchScenes()
		{
			if (PreppedSwitchScenes)
				throw new Exception();
			B1NARYConsole.Log(nameof(SceneManager), "Prepping to switch scenes,"
				+ " Launching methods tied to it.");
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
		public static float Progression { get; private set; } = 1f;

		/// <summary>
		/// Invokes <see cref="SwitchedScenes"/> and clear it. Also loads the scene
		/// right before/during scene switch.
		/// </summary>
		/// <param name="sceneName">Name of the scene to switch.</param>
		/// <returns> When the scene is finished loading. </returns>
		private static Task SwitchScenes(string sceneName)
		{
			if (!PreppedSwitchScenes)
				throw new Exception();
			Progression = 0f;
			PreppedSwitchScenes = false;
			UnitySceneManager.LoadScene(sceneName);
			IEnumerable<Action<string>> events = SwitchedScenes.GetInvocationList()
				.Cast<Action<string>>();//.Select(action => Task.Run(() => action.Invoke(sceneName)));
			float iteration = 1 / events.Count();
			SwitchedScenes = null;
			IEnumerable<Task> taskEvents = events.Select(action => Task.Run(() => 
				{ action.Invoke(sceneName); Progression += iteration; }));
			return Task.WhenAll(taskEvents)
				.ContinueWith(task => Progression = 1f, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
		public static event Action<string> SwitchedScenes;
	}
}