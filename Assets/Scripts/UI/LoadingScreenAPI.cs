namespace B1NARY.UI
{
	using System;
	using System.Collections;
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
		/// <summary>
		/// Shows the progression, only comes into play when switching scenes and
		/// messing with transitions. 1 by default.
		/// </summary>
		public static float Progression { get; private set; } = 1f;
		
		public static void LoadObjects(IEnumerable<SceneManagerEvent> actions, string sceneName)
		{
			Progression = 0f;
			float iteration = 1f / actions.Count();
			foreach (SceneManagerEvent method in actions)
			{
				Debug.Log($"Load Status: {Progression}. \ntarget method '{method.Target}' with name '{method.Method.Name}'");
				method.Invoke(sceneName);
				Progression += iteration;
			}
			Progression = 1f;
			Debug.Log("Loaded all events");
		}

		
	}
}
