namespace B1NARY.CharacterManagement
{
	using System;
	using UnityEngine;

	public class FollowCube : MonoBehaviour
	{
		public static bool EnableFollow { get; set; } = true;
		private void Awake()
		{
			CharacterManager.Instance.ActiveCharacterChanged += ChangeToNewCharacter;
			ChangeToNewCharacter(CharacterManager.Instance.ActiveCharacter);
		}

		private void ChangeToNewCharacter(Character? nullableCharacter)
		{
			if (!EnableFollow)
			{
				gameObject.SetActive(false);
				return;
			}
			if (!nullableCharacter.HasValue)
			{
				gameObject.SetActive(false);
				return;
			}

			Character character = nullableCharacter.Value;
			if (character.controller is IFollowable followable)
			{
				gameObject.SetActive(true);
				transform.SetParent(followable.FollowCubeParent);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			CharacterManager.Instance.ActiveCharacterChanged -= ChangeToNewCharacter;
		}
	}
}