namespace B1NARY
{
	using B1NARY.Audio;
	using B1NARY.Scripting.Experimental;
	using B1NARY.UI;
	using System;
	using UnityEngine;

	public class EmptyController : MonoBehaviour, ICharacterController
	{
		public static (GameObject @object, EmptyController emptyController) Instantiate(Transform parent, string name)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			var emptyController = gameObject.AddComponent<EmptyController>();
			return (gameObject, emptyController);
		}

		private void Awake()
		{
			VoiceActorHandler = new VoiceActorHandler(gameObject);
			if (string.IsNullOrEmpty(CharacterName))
				CharacterName = gameObject.name;
		}
		public VoiceActorHandler VoiceActorHandler { get; private set; }
		public string CharacterName
		{
			get => gameObject.name;
			set => gameObject.name = value;
		}
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.lineData);
			VoiceActorHandler.Play(line);
		}

		void ICharacterController.SetPosition(float xCoord)
		{
			
		}
		void ICharacterController.SetPositionOverTime(float xCoord, float time, bool smooth)
		{
			
		}
		string ICharacterController.CurrentAnimation => throw new NotSupportedException();
		string ICharacterController.CurrentExpression => throw new NotSupportedException();
		void ICharacterController.ChangeExpression(string expressionName) => throw new NotSupportedException();
		void ICharacterController.PlayAnimation(string animationName) => throw new NotSupportedException();
	}
}