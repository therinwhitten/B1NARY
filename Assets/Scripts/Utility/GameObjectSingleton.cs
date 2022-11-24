namespace B1NARY
{
	using System.Linq;
	using UnityEngine;
	public sealed class GameObjectSingleton : MonoBehaviour
	{
		public string gameObjectKey = "Singleton Object";
		private void Awake()
		{
			if (FindObjectsOfType<GameObjectSingleton>().Count(obj => obj.gameObjectKey == gameObjectKey) > 1)
			{
				Debug.LogError($"A '{name}' gameobject with singleton key '{gameObjectKey}' already exists in the scene! Deleting..");
				DestroyImmediate(gameObject); // To prevent other singletons and commands to run.
				return;
			}
		}
	}
}