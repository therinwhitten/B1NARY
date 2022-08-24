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

		private Task testTask = Task.CompletedTask;
		public override void OnInspectorGUI()
		{
			var serializedObject = new SerializedObject(dialogueSystem);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.speakerBox)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DialogueSystem.textBox)));
			if (GUILayout.Button("Test Example Dialogue System"))
				if (Application.isPlaying)
					if (testTask.IsCompleted)
						testTask = Test("Beep Boop I am the <b>tester</b> with the name {0}", "Tester Boogyman");
					else
						Debug.LogError("Cannot play test, its already in succession!");
				else
					Debug.LogError("Cannot play test, it may be not in play mode!");
			serializedObject.ApplyModifiedProperties();
		}

		public static async Task Test(string message, string speaker)
		{
			var dialogueSystem = DialogueSystem.Instance;
			Debug.Log($"Testing {nameof(DialogueSystem)}, fading in instantly.");
			dialogueSystem.FadeIn(0f);
			await Task.Delay(1000);
			Debug.Log($"Testing Typewriter Rich Text with name '{speaker}', " +
				$"\nExpected result: {message}");
			dialogueSystem.CurrentSpeaker = speaker;
			dialogueSystem.Say(message.Replace("{0}", speaker));
			await dialogueSystem.SpeakingTask;
			Debug.Log("Testing completed and reached to end.");
		}
	}
}
