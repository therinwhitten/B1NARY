#if UNITY_EDITOR
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
			if (DialogueSystem.HasInstance && CharacterManager.Instance.ActiveCharacter.HasValue)
				EditorGUILayout.LabelField($"Is current speaker: {CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterName == characterController.CharacterName}");
			if (characterController.VoiceData != null)
				AudioControllerEditor.DisplayAudioData(characterController.VoiceData);
		}

		public override bool RequiresConstantRepaint() => 
			characterController != null 
			&& characterController.VoiceData != null 
			? characterController.VoiceData.IsPlaying : false;
	}
}
#endif