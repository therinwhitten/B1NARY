namespace B1NARY.CharacterManagement.Editor
{
	using B1NARY.Audio.Editor;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using UnityEditor;


	[CustomEditor(typeof(ICharacterController), true)]
	public abstract class ControllerEditor : Editor
	{
		protected ICharacterController characterController;
		private void Awake() => characterController = (ICharacterController)target;

		public override void OnInspectorGUI()
		{
			if (DialogueSystem.HasInstance)
				EditorGUILayout.LabelField($"Is current speaker: {DialogueSystem.Instance.CurrentSpeaker == characterController.CharacterName}");
			if (characterController.VoiceData != null)
				AudioControllerEditor.DisplayAudioData(characterController.VoiceData);
		}

		public override bool RequiresConstantRepaint() => 
			characterController != null 
			&& characterController.VoiceData != null 
			? characterController.VoiceData.IsPlaying : false;
	}

	[CustomEditor(typeof(EmptyController))]
	public class EmptyControllerEditor : ControllerEditor
	{
	
	}
	
	[CustomEditor(typeof(CharacterScript))]
	public class CharacterScriptEditor : ControllerEditor
	{
	
	}
}