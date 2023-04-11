namespace B1NARY
{
	using System.Linq;
	using UnityEngine;
	public sealed class GameObjectSingleton : MonoBehaviour
	{
		public enum DuplicateBehaviour
		{
			DeleteFuture,
			DeleteCurrent
		}
		[Tooltip("What to do if a duplicate (or with the same name & gameobject) is found")]
		public DuplicateBehaviour duplicateBehaviour;
		private void Awake()
		{
			GameObjectSingleton[] singletons = FindObjectsOfType<GameObjectSingleton>()
				.Where(obj => obj.gameObject.name == gameObject.name).ToArray();
			if (singletons.Length > 1)
			{
				if (duplicateBehaviour == DuplicateBehaviour.DeleteFuture)
				{
					Debug.Log($"A '{name}' gameobject with singleton key '{gameObject.name}' already exists in the scene! Deleting..");
					DestroyImmediate(gameObject); // To prevent other singletons and commands to run.
					return;
				}
				else if (duplicateBehaviour == DuplicateBehaviour.DeleteCurrent)
				{
					Debug.Log($"A '{name}' gameobject with singleton key '{gameObject.name}' already exists in the scene! Deleting newest..");
					for (int i = 0; i < singletons.Length && !ReferenceEquals(this, singletons[i]); i++)
					{
						DestroyImmediate(singletons[i].gameObject); // To prevent other singletons and commands to run.
					}
				}
			}
		}
	}
}