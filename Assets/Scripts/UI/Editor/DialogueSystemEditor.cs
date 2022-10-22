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
		//private CoroutineWrapper testWrapper;
		private void Awake()
		{
			dialogueSystem = (DialogueSystem)target;
		}
		public override void OnInspectorGUI()
		{
			var serializedObject = new SerializedObject(dialogueSystem);
			//EditorGUILayout.LabelField("Current Font: " + dialogueSystem.CurrentFontAsset.name);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.speakerBox)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.textBox)));
			//if (GUILayout.Button("Test Example Dialogue System"))
			//	if (Application.isPlaying)
			//		if (CoroutineWrapper.IsNotRunningOrNull(testWrapper))
			//			testWrapper = Test("Beep Boop <i>I am the <b>tester man</b></i> with the name {0}", "Tester Boogyman");
			//		else
			//			Debug.LogError("Cannot play test, its already in succession!");
			//	else
			//		Debug.LogError("Cannot play test, it may be not in play mode!");
			serializedObject.ApplyModifiedProperties();
		}

	}
}
