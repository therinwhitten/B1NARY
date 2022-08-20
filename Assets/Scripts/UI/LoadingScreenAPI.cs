namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.UI;

	public static class LoadingScreenAPI
	{
		public static float Progression { get; private set; } = 1f;
		
		public static Task LoadObjects(IEnumerable<Action> actions)
		{
			var thread = new Thread(Thread);
			async void Thread()
			{
				Progression = 0f;
				float iteration = 1f / actions.Count();
				IEnumerable<Task> taskActions = actions.Select(action =>
				Task.Run(() => { action.Invoke(); Progression += iteration; }));
				await Task.WhenAll(taskActions).ContinueWith(task => Progression = 1f, 
					TaskContinuationOptions.OnlyOnRanToCompletion);
			}
			while (thread.ThreadState == ThreadState.Running)
				Task.Yield();
			return Task.CompletedTask;
		}

		
	}
}
