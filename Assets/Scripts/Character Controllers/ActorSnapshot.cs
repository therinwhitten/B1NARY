namespace B1NARY.CharacterManagement
{
	using System;
	using System.Collections.Generic;
	using OVSXmlSerializer;
	using UnityEngine;

	[Serializable]
	public struct ActorSnapshot
	{
		public static Dictionary<string, Func<ActorSnapshot, Character>> snapshot = new Dictionary<string, Func<ActorSnapshot, Character>>();
		public static ActorSnapshot[] GetCurrentSnapshots()
		{
			ActorSnapshot[] characterSnapshots = new ActorSnapshot[CharacterManager.Instance.CharactersInScene.Count];
			using (var enumerator = CharacterManager.Instance.CharactersInScene.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
					characterSnapshots[i] = enumerator.Current.Value.controller.Serialize();
			return characterSnapshots;
		}

		public string name;
		public string gameObjectName;
		public string expression;
		public string animation;
		public Vector2 screenPosition;
		public bool selected;
		[XmlAttribute("type")]
		public string characterTypeKey;

		public ActorSnapshot(IActor controller)
		{
			name = controller.CharacterName;
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