namespace B1NARY.Editor
{
	using UnityEditor;
	using B1NARY.Audio;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	[CustomEditor(typeof(AudioController))]
	public class AudioControllerEditor : Editor
	{
		private AudioController audioController;
		private void Awake() => audioController = (AudioController)target;


		public override void OnInspectorGUI()
		{
			if (audioController.CurrentSoundLibrary != null)
				EditorGUILayout.LabelField($"Current Library: '{audioController.CurrentSoundLibrary.name}'");
			if (audioController.ActiveAudioTrackers == null || !Application.isPlaying)
			{
				EditorGUILayout.LabelField("Reading data is only on runtime!");
				return;
			}
			EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioController.ActiveAudioTrackers.Count}", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			using (IEnumerator<KeyValuePair<(string name, int index), AudioTracker>> enumerator = audioController.ActiveAudioTrackers.GetEnumerator())
				while (enumerator.MoveNext())
				{
					Rect fullNameRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
						toggleRect = new Rect(fullNameRect) { width = 20f };
					fullNameRect.xMin += 20f;
					enumerator.Current.Value.IsPlaying = EditorGUI.Toggle(toggleRect, enumerator.Current.Value.IsPlaying);
					EditorGUI.LabelField(fullNameRect, enumerator.Current.Value.ClipName, EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					Rect barRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 24f));
					barRect.xMin += 2f;
					barRect.xMax -= 2f;
					EditorGUI.ProgressBar(barRect, enumerator.Current.Value.CompletionPercent, enumerator.Current.Value.TimeInfo);
					enumerator.Current.Value.Loop = EditorGUILayout.Toggle("Looping", enumerator.Current.Value.Loop);
					enumerator.Current.Value.Pitch = EditorGUILayout.Slider("Pitch", enumerator.Current.Value.Pitch, 0f, 3f);
					EditorGUILayout.LabelField($"Auto-Disposing: {enumerator.Current.Value.CreateAutoDisposableCoroutine}");
				}
			EditorGUI.indentLevel -= 2;
		}
		public override bool RequiresConstantRepaint() => audioController != null && audioController.ActiveAudioTrackers != null ? audioController.ActiveAudioTrackers.Count > 0 : false;
	}
}