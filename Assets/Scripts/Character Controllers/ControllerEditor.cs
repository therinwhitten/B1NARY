#if UNITY_EDITOR
namespace B1NARY.CharacterManagement.Editor
{
	using B1NARY.Audio.Editor;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using UnityEditor;


	[CustomEditor(typeof(IActor), true)]
	public abstract class ControllerEditor : Editor
	{
		protected IActor characterController;
		private void Awake() => characterController = (IActor)target;

		public override void OnInspectorGUI()
		{
			if (DialogueSystem.HasInstance && CharacterManager.Instance.ActiveCharacter.HasValue)
				EditorGUILayout.LabelField($"Is current speaker: {CharacterManager.Instance.ActiveCharacter.Value.controller.CharacterName == characterController.CharacterName}");
		}

		public override bool RequiresConstantRepaint() => 
			characterController != null 
			&& characterController.Mouths != null 
			&& characterController.Mouths.Count > 0
			&& characterController.Mouths[0].IsPlaying;
	}
}
#endif