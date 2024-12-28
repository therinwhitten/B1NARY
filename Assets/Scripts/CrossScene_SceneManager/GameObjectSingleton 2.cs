namespace B1NARY
{
	using System.Linq;
	using UnityEngine;
	public class GameObjectSingleton : MonoBehaviour
	{
		public enum DuplicateBehaviour
		{
			DeleteFuture,
			DeleteCurrent
		}
		public enum CrossSceneBehaviour
		{
			KeepObject,
			RemoveObject
		}
		public DuplicateBehaviour CurrentDuplicateBehaviour
		{
			get => m_duplicateBehaviour;
			set => m_duplicateBehaviour = value;
		}
		[SerializeField, HideInInspector]
		private DuplicateBehaviour m_duplicateBehaviour;
		public CrossSceneBehaviour CurrentCrossSceneBehaviour
		{
			get => m_crossSceneBehaviour;
			set => m_crossSceneBehaviour = value;
		}
		[SerializeField, HideInInspector]
		private CrossSceneBehaviour m_crossSceneBehaviour;

		protected virtual void Awake()
		{
			GameObjectSingleton[] singletons = FindObjectsOfType<GameObjectSingleton>()
				.Where(obj => obj.gameObject.name == gameObject.name).ToArray();
			if (singletons.Length > 1)
			{
				if (CurrentDuplicateBehaviour == DuplicateBehaviour.DeleteFuture)
				{
					Debug.Log($"A '{name}' gameobject with singleton key '{gameObject.name}' already exists in the scene! Deleting..");
					DestroyImmediate(gameObject); // To prevent other singletons and commands to run.
					return;
				}
				else if (CurrentDuplicateBehaviour == DuplicateBehaviour.DeleteCurrent)
				{
					Debug.Log($"A '{name}' gameobject with singleton key '{gameObject.name}' already exists in the scene! Deleting newest..");
					for (int i = 0; i < singletons.Length && !ReferenceEquals(this, singletons[i]); i++)
					{
						DestroyImmediate(singletons[i].gameObject); // To prevent other singletons and commands to run.
					}
				}
			}
			if (CurrentCrossSceneBehaviour == CrossSceneBehaviour.KeepObject)
			{
				DontDestroyOnLoad(gameObject);
			}
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(GameObjectSingleton), true)]
	public class GameObjSingleton : Editor
	{
		public override void OnInspectorGUI()
		{
			GameObjectSingleton singleton = (GameObjectSingleton)target;
			singleton.CurrentDuplicateBehaviour = DirtyAuto.Popup(target, new GUIContent("Duplicate Behaviour"), singleton.CurrentDuplicateBehaviour);
			singleton.CurrentCrossSceneBehaviour = DirtyAuto.Popup(target, new GUIContent("Cross Scene Behaviour"), singleton.CurrentCrossSceneBehaviour);
		}
	}
}
#endif