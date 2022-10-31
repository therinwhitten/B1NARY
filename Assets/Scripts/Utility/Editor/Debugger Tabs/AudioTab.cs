namespace B1NARY.Editor.Debugger
{
	using System.Globalization;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.Audio;

	public sealed class AudioTab : DebuggerTab
	{
		public override GUIContent Name => new GUIContent("Audio");
		public override bool ConstantlyRepaint => true;

		public override void DisplayTab()
		{
			var audioHandler = AudioController.Instance;
			EditorGUILayout.LabelField($"Current Library: {(audioHandler == null || audioHandler.CurrentSoundLibrary == null ? "Null" : audioHandler.CurrentSoundLibrary.name)}");
			ShowSoundData(audioHandler);
		}
		private void ShowSoundData(AudioController audioHandler)
		{
			bool hasAudioHandlerValue = audioHandler != null;
			EditorGUILayout.LabelField(new GUIContent($"Sound Count : {(hasAudioHandlerValue ? audioHandler.ActiveAudioTrackers.Count.ToString(CultureInfo.CurrentCulture) : "NaN")}"), EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			if (hasAudioHandlerValue)
				foreach (AudioTracker coroutine in audioHandler.ActiveAudioTrackers.Values)
				{
					EditorGUILayout.LabelField(coroutine.ClipName, EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					Rect progressRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
					progressRect = EditorGUI.IndentedRect(progressRect);
					progressRect.x += 4;
					progressRect.width -= 24;
					EditorGUI.ProgressBar(progressRect, coroutine.CompletionPercent, $"Time: {coroutine.PlayedSeconds.TotalMinutes:N2} / {coroutine.TotalSeconds.TotalSeconds:N2}");
					//EditorGUILayout.Space();
					EditorGUILayout.LabelField($"Volume: {coroutine.Volume:N2}");
					EditorGUILayout.LabelField($"Pitch: {coroutine.Pitch:N2}");
					EditorGUILayout.LabelField($"Looping: {coroutine.Loop}");
					EditorGUILayout.Space();
					EditorGUI.indentLevel--;
				}
			EditorGUI.indentLevel--;
		}
	}
}