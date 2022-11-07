namespace B1NARY.Editor
{
	using B1NARY.UI;
	using System.Threading.Tasks;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(DialogueSystem))]
	public class DialogueSystemEditor : Editor
	{
		private DialogueSystem dialogueSystem;
		private void Awake()
		{
			dialogueSystem = (DialogueSystem)target;
		}
		public override void OnInspectorGUI()
		{
			//EditorGUILayout.LabelField("Current Font: " + dialogueSystem.CurrentFontAsset.name);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.speakerBox)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.textBox)));
			dialogueSystem.TicksPerCharacter = DirtyAuto.Slider(target, new GUIContent("Ticks Waited Per Character", "How long the text box will wait per character within milliseconds, with 0 being instantaneous. 30 by default."), dialogueSystem.TicksPerCharacter, 0, 200);
			serializedObject.ApplyModifiedProperties();
		}

	}
}
