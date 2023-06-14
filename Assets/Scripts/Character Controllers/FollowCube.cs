namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using System;
	using System.Net.Sockets;
	using UnityEngine;

	public class FollowCube : CachedMonobehaviour
	{
		public static CommandArray Commands => new CommandArray()
		{
			["togglecube"] = (Action<string>)((booleanRaw) =>
			{
				FollowCube[] cubes = Resources.FindObjectsOfTypeAll<FollowCube>();
				bool parsedBoolean = bool.Parse(booleanRaw);
				for (int i = 0; i < cubes.Length; i++)
					cubes[i].gameObject.SetActive(parsedBoolean);
			}),
			["movecube"] = (Action<string, string>)((x, y) =>
			{
				Vector3 localPosition = new Vector3(float.Parse(x), float.Parse(y), 0f);
				FollowCube[] cubes = FindObjectsOfType<FollowCube>();
				for (int i = 0; i < cubes.Length; i++)
					cubes[i].transform.localPosition = localPosition;
			}),
			["toggleautocube"] = (Action<string>)((booleanRaw) => 
			{
				FollowCube[] cubes = FindObjectsOfType<FollowCube>();
				bool parsedBoolean = bool.Parse(booleanRaw);
				for (int i = 0; i < cubes.Length; i++)
					cubes[i].EnableAutoFollow = parsedBoolean;
			}),
		};
		public bool EnableAutoFollow
		{
			get => m_enableAutoFollow;
			set
			{
				if (m_enableAutoFollow == value)
					return;
				m_enableAutoFollow = value;
				if (m_enableAutoFollow)
					ChangeToNewCharacter(CharacterManager.Instance.ActiveCharacter);
				else
					ChangeToNewCharacter(null);
			}
		}
		[SerializeField]
		internal bool m_enableAutoFollow = false;


		private void Awake()
		{
			CharacterManager.Instance.ActiveCharacterChanged += ChangeToNewCharacter;
		}
		private void OnEnable()
		{
			ChangeToNewCharacter(CharacterManager.Instance.ActiveCharacter);
		}

		[NonSerialized, HideInInspector]
		internal Vector2 TargetPosition = Vector2.zero;
		private Vector3 velocity = Vector3.zero;
		private void ChangeToNewCharacter(Character? nullableCharacter)
		{
			if (!EnableAutoFollow || !enabled)
			{
				SetDisable();
				return;
			}
			if (!nullableCharacter.HasValue)
			{
				SetDisable();
				return;
			}

			Character character = nullableCharacter.Value;
			if (character.controller is IFollowable followable && followable.FollowCubeParent != null)
				TargetPosition = followable.FollowCubeParent.position;
			else
				SetDisable();

			void SetDisable()
			{
				TargetPosition = Vector2.zero;
			}
		}
		private void Update()
		{
			Transform transform = GetComponent<Transform>();
			transform.position = Vector3.SmoothDamp(transform.position,
				TargetPosition, ref velocity, 0.35f);
		}

		private void OnDestroy()
		{
			if (CharacterManager.TryGetInstance(out var instance))
				instance.ActiveCharacterChanged -= ChangeToNewCharacter;
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using B1NARY.Editor;
	using System;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(FollowCube))]
	public class FollowCubeEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			FollowCube cube = (FollowCube)target;
			bool outFollow = DirtyAuto.Toggle(target, new GUIContent("Auto Follow", "Using the head transforms that are inputted in, it moves the follow cube to the direction."), cube.EnableAutoFollow);
			if (outFollow != cube.EnableAutoFollow && Application.isPlaying)
				cube.EnableAutoFollow = outFollow;
			else
				cube.m_enableAutoFollow = outFollow;
			EditorGUILayout.Vector3Field("Target Pos (readonly)", cube.TargetPosition);
		}
	}
}
#endif