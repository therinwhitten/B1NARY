namespace B1NARY
{
	using UnityEngine;
	using System;
	using System.Threading.Tasks;
	using System.Diagnostics;
	using UI;

	[RequireComponent(typeof(FadeController))]
	public class MainMenuCommands : MonoBehaviour
	{
		// public int x, y;
		// [SerializeField] int maxX, maxY;
		// bool keyDown;
		[SerializeField]
		GameObject optionsMenu;
		[SerializeField]
		GameObject dialogueBox;
		FadeController fadeController;
		[SerializeField]
		private FadeController transitionHandler;

		private void Start()
		{
			fadeController = GetComponent<FadeController>();
		}

		// BUTTON BEHAVIOURS
		private static Task NewGameTask = Task.CompletedTask;
		public void NewGame()
		{
			B1NARYConsole.Log(name, "Initialized new game sequence.");
			NewGameTask = Taskable();

			async Task Taskable()
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				B1NARYConsole.Log(name, "Stage 1", "Starting Pre-loading sequence.");
				await Task.WhenAll(PreTasks());
				B1NARYConsole.Log(name, "Stage 2", "Starting Loading sequence.");
				await Task.WhenAll(LoadTasks());
				B1NARYConsole.Log(name, "Stage 3", "Starting Post-Loading sequence.");
				await Task.WhenAll(PostTasks());
				stopwatch.Stop();
				B1NARYConsole.Log(name, $"Loading Lasted: {stopwatch.Elapsed}");
			}
			/*
			StartCoroutine(CommitTasks(PreTasks(), CommitTasks(LoadTasks(), CommitTasks(PostTasks(), After()))));

			IEnumerator CommitTasks(Task[] tasks, IEnumerator nextTask = null)
			{
				const int maxCycles = 5000;
				Task committingTasks = Task.WhenAll(tasks);
				for (int cycles = 0; !committingTasks.IsCompleted && cycles < maxCycles; cycles++)
					yield return new WaitForEndOfFrame();
				if (nextTask != null)
					while (nextTask.MoveNext())
						yield return nextTask.Current;
			}
			IEnumerator After()
			{
				yield break;
			}
			*/
			Task[] PreTasks() => new Task[]
			{
				fadeController.FadeOut(0.5f),
				//transitionHandler.FadeIn(1f),
			};
			Task[] LoadTasks() => new Task[]
			{
				Task.Run(() => B1NARYConsole.Log(nameof(MainMenuCommands),
					"Loading systems..")),
				Task.WhenAny(Task.Run(DialogueSystem.Initialize), Task.Delay(100)),
				FadeController.FadeInAndActivate(dialogueBox.GetComponent<FadeController>(), 0.1f),
			};
			Task[] PostTasks() => new Task[]
			{
				Task.Run(() => B1NARYConsole.Log(nameof(MainMenuCommands),
					"Task Complete! Starting misc systems.")),
				//transitionHandler.FadeOut(1f),
				ScriptParser.Instance.Initialize(),
				Task.Run(() => { Task.Delay(1000); Destroy(gameObject); }),
			};
		}
		public void LoadGame()
		{
			throw new NotImplementedException();
		}
		public void Options()
		{
			throw new NotImplementedException();
		}
		public void Exit() => Application.Quit();
	}
}