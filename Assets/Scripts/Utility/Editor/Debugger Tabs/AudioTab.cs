namespace B1NARY.Editor.Debugger
{
	using System.Globalization;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.Audio;

	public sealed class AudioTab : DebuggerTab
	{
		public override string Name => "Audio";

		public override void DisplayTab()
		{
			var audioHandler = Object.FindObjectOfType<AudioHandler>();
			EditorGUILayout.LabelField($"Current Library: {(audioHandler == null || audioHandler.CustomAudioData == null ? "Null" : audioHandler.CustomAudioData.name)}");
			ShowSoundData(audioHandler);
		}
		private void ShowSoundData(AudioHandler audioHandler)
		{
			bool hasAudioHandlerValue = audioHandler != null;
			EditorGUILayout.LabelField(new GUIContent($"Sound Count : {(hasAudioHandlerValue ? audioHandler.SoundCoroutineCache.Count.ToString(CultureInfo.CurrentCulture) : "NaN")}"), EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			if (hasAudioHandlerValue)
				foreach (AudioTracker coroutine in audioHandler.SoundCoroutineCache.Values)
				{
					EditorGUILayout.LabelField(coroutine.AudioSource.clip.name, EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					float timePercent = coroutine.AudioSource.time / coroutine.AudioSource.clip.length;
					Rect progressRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
					progressRect = EditorGUI.IndentedRect(progressRect);
					progressRect.x += 4;
					progressRect.width -= 24;
					EditorGUI.ProgressBar(progressRect, timePercent, $"Time: {coroutine.AudioSource.time:N2} / {coroutine.AudioSource.clip.length:N2}");
					//EditorGUILayout.Space();
					EditorGUILayout.LabelField($"Volume: {coroutine.AudioClip.volume:N2}");
					EditorGUILayout.LabelField($"Pitch: {coroutine.AudioClip.pitch:N2}");
					EditorGUILayout.LabelField($"Looping: {coroutine.AudioClip.loop}");
					EditorGUILayout.Space();
					EditorGUI.indentLevel--;
				}
			EditorGUI.indentLevel--;
		}
	}
}