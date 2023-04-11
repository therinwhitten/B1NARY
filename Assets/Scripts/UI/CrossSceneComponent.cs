namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Linq;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public sealed class CrossSceneComponent : GameObjectSingleton
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			GameObject instance = Resources.Load<GameObject>("Cross Scene");
			instance = Object.Instantiate(instance);
			Debug.Log("Cross Scene Loaded!");
		}
	}
}