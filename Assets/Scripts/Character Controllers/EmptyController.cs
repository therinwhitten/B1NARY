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
		public string CharacterName
		{
			get => gameObject.name;
			set => gameObject.name = value;
		}
		string ICharacterController.OldCharacterName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public void SayLine(ScriptLine line)
		{
			DialogueSystem.Instance.Say(line.RawLine);
			VoiceData.Play(line);
		}

		float ICharacterController.HorizontalPosition { get => 0f; set { } }
		void ICharacterController.SetPositionOverTime(float xCoord, float time)
		{
			
		}
		string ICharacterController.CurrentAnimation
		{
			get => string.Empty;
			set => throw new NotSupportedException();
		}

		string ICharacterController.CurrentExpression
		{
			get => string.Empty;
			set => throw new NotSupportedException();
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