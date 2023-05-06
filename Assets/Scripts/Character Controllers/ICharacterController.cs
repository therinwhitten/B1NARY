namespace B1NARY.CharacterManagement
{
	using B1NARY.Audio;
	using B1NARY.Scripting;
	using OVSXmlSerializer;
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public interface ICharacterController
	{
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
		string CharacterTypeKey { get; }
		CharacterSnapshot Serialize();
		void Deserialize(CharacterSnapshot snapshot);
	}
	public interface IFollowable
	{
		Transform FollowCubeParent { get; }
	}

	[Serializable]
	public struct CharacterSnapshot
	{
		public static Dictionary<string, Func<CharacterSnapshot, Character>> snapshot = new Dictionary<string, Func<CharacterSnapshot, Character>>();
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
		[XmlAttribute("type")]
		public string characterTypeKey;

		public CharacterSnapshot(ICharacterController controller)
		{
			name = controller.CharacterName;
			gameObjectName = controller.GameObjectName;
			expression = controller.CurrentExpression;
			animation = controller.CurrentAnimation;
			horizontalPosition = controller.HorizontalPosition;
			selected = controller.Selected;
			characterTypeKey = controller.CharacterTypeKey; 
		}
		public bool Load(out Character? character)
		{
			if (snapshot.TryGetValue(characterTypeKey, out var action))
			{
				character = action.Invoke(this);
				return true;
			}
			character = null;
			return false;
		}
	}
}