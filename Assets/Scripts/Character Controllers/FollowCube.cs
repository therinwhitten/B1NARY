namespace B1NARY.CharacterManagement
{
	using B1NARY.Scripting;
	using System;
	using System.Net.Sockets;
	using UnityEngine;

	[RequireComponent(typeof(RectTransform))]
	public class FollowCube : MonoBehaviour
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
				Vector2 localPosition = new Vector2(float.Parse(x), float.Parse(y));
				FollowCube[] cubes = FindObjectsOfType<FollowCube>();
				for (int i = 0; i < cubes.Length; i++)
					cubes[i].MoveToPosition(localPosition);
			}),
			["toggleautocube"] = (Action<string>)((booleanRaw) =>
			{
				FollowCube[] cubes = FindObjectsOfType<FollowCube>();
				bool parsedBoolean = bool.Parse(booleanRaw);
				for (int i = 0; i < cubes.Length; i++)
					cubes[i].AutoSwitch = parsedBoolean;
			}),
		};

		[HideInInspector, SerializeField]
		private RectTransform RectTransform;
		private void Reset()
		{
			RectTransform = GetComponent<RectTransform>();
		}

		public bool AutoSwitch = false;

		private void Awake()
		{
			EqualizeTransformAnchors();
			CharacterManager.Instance.ActiveCharacterChanged += AutoSwitchCharacter;
		}
		private void OnDestroy()
		{
			if (CharacterManager.TryGetInstance(out var manager))
				manager.ActiveCharacterChanged -= AutoSwitchCharacter;
		}


		public FollowState followState = FollowState.StayingAtPosition;

		private void Update()
		{
			switch (followState)
			{
				case FollowState.FollowingHead:
					CharacterUpdate();
					break;
				case FollowState.StayingAtPosition:
					PositionUpdate();
					break;
			}
		}
		/// <summary>
		/// Equalizes the transform anchors in <see cref="RectTransform"/> 
		/// </summary>
		public void EqualizeTransformAnchors()
		{
			RectTransform.localPosition = Vector3.zero;
			Vector2 anchorMin = RectTransform.anchorMin;
			anchorMin.x = (RectTransform.anchorMin.x + RectTransform.anchorMax.x) * 0.5f;
			anchorMin.y = (RectTransform.anchorMin.y + RectTransform.anchorMax.y) * 0.5f;
			RectTransform.anchorMin = anchorMin;
			RectTransform.anchorMax = anchorMin;
		}


		internal Vector2 AnchorPosition
		{
			get => RectTransform.anchorMax;
			set
			{
				RectTransform.anchorMin = value;
				RectTransform.anchorMax = value;
			}
		}
		private Vector2 anchorVelocity = Vector2.zero;

		private Vector2 velocity = Vector2.zero;


		private Character? character;
		private void AutoSwitchCharacter(Character? character)
		{
			if (!AutoSwitch)
				return;
			SwitchToCharacter(character);
		}
		public void SwitchToCharacter(Character? character)
		{
			if (character == null)
				return;
			if (!(character.Value.controller is IFollowable))
				return;
			this.character = character;
			followState = FollowState.FollowingHead;
		}
		private void CharacterUpdate()
		{
			IFollowable followable = character.Value.controller as IFollowable;
			// Convert the anchor to zero
			AnchorPosition = Vector2.SmoothDamp(AnchorPosition, Vector2.zero, ref anchorVelocity, 0.35f);
			RectTransform.position = Vector2.SmoothDamp(RectTransform.position, followable.FollowCubeParent.position, ref velocity, 0.35f);
		}

		private Vector2 targetPosition = Vector2.one * 0.5f;
		public void MoveToPosition(Vector2 normalizedPosition)
		{
			targetPosition = normalizedPosition;

		}
		private void PositionUpdate()
		{
			AnchorPosition = Vector2.SmoothDamp(AnchorPosition, targetPosition, ref anchorVelocity, 0.35f);
			RectTransform.localPosition = Vector2.SmoothDamp(RectTransform.localPosition, Vector2.zero, ref velocity, 0.35f);
		}

		/*
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
				Vector2 localPosition = new Vector2(float.Parse(x), float.Parse(y));
				FollowCube[] cubes = FindObjectsOfType<FollowCube>();
				for (int i = 0; i < cubes.Length; i++)
				{
					cubes[i].m_rectTransform.anchorMax = localPosition;
					cubes[i].m_rectTransform.anchorMin = localPosition;
				}
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
		internal RectTransform m_rectTransform;


		private void Awake()
		{
			CharacterManager.Instance.ActiveCharacterChanged += ChangeToNewCharacter;
			m_rectTransform = GetComponent<RectTransform>();
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
		*/
	}

	public enum FollowState : byte
	{
		FollowingHead,
		StayingAtPosition,
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
			cube.AutoSwitch = DirtyAuto.Toggle(target, new GUIContent("Auto Switch", "Using the head transforms that are inputted in, it moves the follow cube to the direction."), cube.AutoSwitch);
			//EditorGUILayout.Vector3Field("Target Pos (readonly)", cube.);
		}
	}
	
}
#endif