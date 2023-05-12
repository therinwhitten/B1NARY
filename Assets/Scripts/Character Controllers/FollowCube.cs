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
				Vector2 localPosition = new Vector2(float.Parse(x), float.Parse(y));
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
			get => enableAutoFollow;
			set
			{
				if (enableAutoFollow == value)
					return;
				enableAutoFollow = value;
				if (enableAutoFollow)
					ChangeToNewCharacter(CharacterManager.Instance.ActiveCharacter);
				else
					ChangeToNewCharacter(null);
			}
		}
		[SerializeField]
		private bool enableAutoFollow = false;


		private void Awake()
		{
			CharacterManager.Instance.ActiveCharacterChanged += ChangeToNewCharacter;
		}
		private void OnEnable()
		{
			ChangeToNewCharacter(CharacterManager.Instance.ActiveCharacter);
		}

		private Vector2 TargetPosition = Vector2.zero;
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
			if (character.controller is IFollowable followable)
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
			GetComponent<Transform>().position =
				Vector3.SmoothDamp(GetComponent<Transform>().position,
				TargetPosition, ref velocity, 0.35f);
		}

		private void OnDestroy()
		{
			if (CharacterManager.TryGetInstance(out var instance))
				instance.ActiveCharacterChanged -= ChangeToNewCharacter;
		}
	}
}