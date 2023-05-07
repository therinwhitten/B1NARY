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
			gameObject.SetActive(false);
		}

		private void ChangeToNewCharacter(Character? nullableCharacter)
		{
			if (!EnableFollow)
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
			{
				gameObject.SetActive(true);
				transform.SetParent(followable.FollowCubeParent);
			}
			else
			{
				SetDisable();
			}

			void SetDisable()
			{
				if (!gameObject.activeSelf)
					return;
				gameObject.SetActive(false);
				transform.SetParent(null);
			}
		}

		private void OnDestroy()
		{
			if (CharacterManager.TryGetInstance(out var instance))
				instance.ActiveCharacterChanged -= ChangeToNewCharacter;
		}
	}
}