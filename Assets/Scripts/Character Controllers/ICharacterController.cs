namespace B1NARY.CharacterManagement
{
	using B1NARY.Audio;
	using B1NARY.Scripting;
	using System;
	using UnityEngine;

	public interface ICharacterController
	{
		bool EmptyCharacter { get; }
		string CharacterName { get; set; }
		string GameObjectName { get; set; }
		VoiceActorHandler VoiceData { get; }
		void SayLine(ScriptLine line);

		float HorizontalPosition { get; set; }
		void SetPositionOverTime(float xCoord, float time);

		string CurrentAnimation { get; set; }
		/// <summary>
		/// The character's current expression. <see langword="null"/> if it has
		/// no current expression or unable to display any as it is invisible.
		/// </summary>
		string CurrentExpression { get; set; }
		bool Selected { get; set; }
		CharacterSnapshot Serialize();
		void Deserialize(CharacterSnapshot snapshot);
	}

	[Serializable]
	public struct CharacterSnapshot
	{
		public static CharacterSnapshot[] GetCurrentSnapshots()
		{
			CharacterSnapshot[] characterSnapshots = new CharacterSnapshot[CharacterController.Instance.charactersInScene.Count];
			int i = 0;
			using (var enumerator = CharacterController.Instance.charactersInScene.GetEnumerator())
				while (enumerator.MoveNext())
				{
					characterSnapshots[i] = enumerator.Current.Value.characterScript.Serialize();
					i += 1;
				}
			return characterSnapshots;
		}

		public string name;
		public string gameObjectName;
		public string expression;
		public string animation;
		public float horizontalPosition;
		public bool selected;
		public bool asEmpty;

		public CharacterSnapshot(ICharacterController controller)
		{
			name = controller.CharacterName;
			gameObjectName = controller.GameObjectName;
			expression = controller.CurrentExpression;
			animation = controller.CurrentAnimation;
			horizontalPosition = controller.HorizontalPosition;
			selected = controller.Selected;
			asEmpty = controller.EmptyCharacter;
		}
		public void Load()
		{
			if (asEmpty)
			{
				var pair = EmptyController.Instantiate(CharacterController.Instance.transform, gameObjectName);
				CharacterController.Instance.charactersInScene.Add(gameObjectName, pair);
			}
			else if (!CharacterController.Instance.SummonCharacter(gameObjectName, horizontalPosition))
			{
				Debug.LogError($"Failure to load {gameObjectName} from data.");
				return;
			}
			CharacterController.Instance.charactersInScene[gameObjectName]
				.characterScript.Deserialize(this);
		}
	}
}