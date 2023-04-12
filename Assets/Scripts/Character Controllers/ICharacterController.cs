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
		string GameObjectName { get; }
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
			CharacterSnapshot[] characterSnapshots = new CharacterSnapshot[CharacterManager.Instance.CharactersInScene.Count];
			using (var enumerator = CharacterManager.Instance.CharactersInScene.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
					characterSnapshots[i] = enumerator.Current.Value.controller.Serialize();
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
				EmptyController.AddTo(CharacterManager.Instance, name).controller.Deserialize(this);
				return;
			}
			Character? @char = CharacterManager.Instance.SummonCharacter(gameObjectName);
			if (@char == null)
			{
				Debug.LogError($"Failure to load {gameObjectName} from data.");
				return;
			}
			@char.Value.controller.HorizontalPosition = horizontalPosition;
			@char.Value.controller.Deserialize(this);
		}
	}
}