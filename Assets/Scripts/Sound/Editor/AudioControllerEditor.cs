namespace B1NARY.Audio.Editor
{
	using UnityEditor;
	using B1NARY.Audio;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	[CustomEditor(typeof(AudioController))]
	public class AudioControllerEditor : Editor
	{
		public static void DisplayAudioData(IAudioInfo audioInfo)
		{
			Rect fullNameRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
				toggleRect = new Rect(fullNameRect) { width = 20f };
			fullNameRect.xMin += toggleRect.xMax + 2;
			audioInfo.IsPlaying = EditorGUI.Toggle(toggleRect, audioInfo.IsPlaying);
			EditorGUI.LabelField(fullNameRect, audioInfo.ClipName, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			Rect barRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 24f));
			barRect.xMin += 2f;
			barRect.xMax -= 2f;
			EditorGUI.ProgressBar(barRect, audioInfo.CompletionPercent(), audioInfo.TimeInfo());
			audioInfo.Volume = EditorGUILayout.Slider("Volume", audioInfo.Volume, 0f, 1f);
			audioInfo.Loop = EditorGUILayout.Toggle("Looping", audioInfo.Loop);
			audioInfo.Pitch = EditorGUILayout.Slider("Pitch", audioInfo.Pitch, 0f, 3f);
			EditorGUI.indentLevel--;
		}
		public static void DisplayAudioController(AudioController audioController)
		{
			if (audioController.ActiveAudioTrackers == null || !Application.isPlaying)
			{
				EditorGUILayout.LabelField("Reading data is only on runtime!");
				return;
			}
			EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioController.ActiveAudioTrackers.Count}", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			using (IEnumerator<KeyValuePair<(string name, int index), AudioTracker>> enumerator = audioController.ActiveAudioTrackers.GetEnumerator())
				while (enumerator.MoveNext())
					DisplayAudioData(enumerator.Current.Value);
			EditorGUI.indentLevel--;
		}


		private AudioController audioController;
		private void Awake() => audioController = (AudioController)target;


		public override void OnInspectorGUI()
		{
			if (audioController.CurrentSoundLibrary != null)
				EditorGUILayout.LabelField($"Current Library: '{audioController.CurrentSoundLibrary.name}'");
			DisplayAudioController(audioController);
		}
		public override bool RequiresConstantRepaint() => audioController != null && audioController.ActiveAudioTrackers != null ? audioController.ActiveAudioTrackers.Count > 0 : false;
	}
}