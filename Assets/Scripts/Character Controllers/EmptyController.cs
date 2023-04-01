namespace B1NARY.CharacterManagement
{
	using B1NARY.Audio;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using UnityEditor;
	using UnityEngine;

	public class EmptyController : MonoBehaviour, ICharacterController
	{
		bool ICharacterController.EmptyCharacter => true;
		public static Character AddTo(CharacterManager characterManager, string name)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.SetParent(characterManager.Transform);
			EmptyController controller = gameObject.AddComponent<EmptyController>();
			characterManager.AddCharacterToDictionary(gameObject, out Character character);
			return character;
		}
		public static (GameObject @object, EmptyController emptyController) Instantiate(Transform parent, string name)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			var emptyController = gameObject.AddComponent<EmptyController>();
			return (gameObject, emptyController);
		}

		private void Awake()
		{
			VoiceData = gameObject.AddComponent<VoiceActorHandler>();
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}
		public VoiceActorHandler VoiceData { get; private set; }
		public string CharacterName { get; set; }
		string ICharacterController.GameObjectName => gameObject.name;
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			VoiceData.Play(line);
		}

		float ICharacterController.HorizontalPosition { get => 0f; set { } }
		void ICharacterController.SetPositionOverTime(float xCoord, float time)
		{
			
		}

		CharacterSnapshot ICharacterController.Serialize()
		{
			CharacterSnapshot snapshot = new CharacterSnapshot(this);
			return snapshot;
		}
		void ICharacterController.Deserialize(CharacterSnapshot snapshot)
		{
			ICharacterController thisInterface = this;
			thisInterface.CurrentExpression = snapshot.expression;
			thisInterface.CharacterName = snapshot.name;
			thisInterface.Selected = snapshot.selected;
			thisInterface.CurrentAnimation = snapshot.animation;
			thisInterface.HorizontalPosition = snapshot.horizontalPosition;
		}

		string ICharacterController.CurrentAnimation
		{
			get => string.Empty;
			set { }
		}

		string ICharacterController.CurrentExpression
		{
			get => string.Empty;
			set { }
		}
		public bool Selected 
		{ 
			get => m_selected; 
			set 
			{
				if (m_selected == value)
					return;
				m_selected = value;

			}
		}
		private bool m_selected = false;
	}
}

#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(EmptyController))]
	public class EmptyControllerEditor : ControllerEditor
	{

	}
}
#endif