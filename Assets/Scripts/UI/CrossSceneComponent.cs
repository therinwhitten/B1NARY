namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Linq;
	using UnityEngine;

	public sealed class CrossSceneComponent : GameObjectSingleton
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			GameObject instance = Resources.Load<GameObject>("Cross Scene");
			instance = Instantiate(instance);
		}
	}
}