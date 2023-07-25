namespace B1NARY.CharacterManagement
{
	using System;
	using System.Collections.Generic;
	using B1NARY.UI.Globalization;
	using OVSXmlSerializer;
	using UnityEngine;

	[Serializable]
	public struct ActorSnapshot
	{
		public static Dictionary<string, Func<ActorSnapshot, Character>> snapshot = new();
		public static ActorSnapshot[] GetCurrentSnapshots()
		{
			ActorSnapshot[] characterSnapshots = new ActorSnapshot[CharacterManager.Instance.CharactersInScene.Count];
			for (int i = 0; i < CharacterManager.Instance.CharactersInScene.Count; i++)
				characterSnapshots[i] = CharacterManager.Instance.CharactersInScene[i].controller.Serialize();
			return characterSnapshots;
		}

		public CharacterNames characterNames;
		public string gameObjectName;
		public string expression;
		public string animation;
		public Vector2 screenPosition;
		public bool selected;
		[XmlAttribute("type")]
		public string characterTypeKey;

		public ActorSnapshot(IActor controller)
		{
			characterNames = controller.CharacterNames;
			gameObjectName = controller.GameObjectName;
			expression = controller.CurrentExpression;
			animation = controller.CurrentAnimation;
			screenPosition = controller.ScreenPosition;
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