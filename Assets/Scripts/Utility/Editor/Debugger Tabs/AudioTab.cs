using System.Globalization;
using UnityEngine;
using UnityEditor;
using System;

public sealed class AudioTab : DebuggerTab
{
	public override string Name => "Audio";

	private bool toggleGroupAudioTab = false;
	public override void DisplayTab()
	{
		bool hasValue = DebuggerWindow.TryGetter<AudioHandler>.TryGetObject(out var audioHandler);
		EditorGUILayout.LabelField($"Current Library: {(audioHandler == null || audioHandler.CustomAudioData == null ? "NaN" : audioHandler.CustomAudioData.name)}");
		ShowSoundData(hasValue, audioHandler);
		AudioEditor();
		EditorGUILayout.EndFoldoutHeaderGroup();
	}
	private void ShowSoundData(bool hasAudioHandlerValue, AudioHandler audioHandler)
	{
		toggleGroupAudioTab =
			EditorGUILayout.BeginFoldoutHeaderGroup(toggleGroupAudioTab, new GUIContent($"Sounds : {(hasAudioHandlerValue ? audioHandler.SoundCoroutineCache.Count.ToString(CultureInfo.CurrentCulture) : "NaN")}"));
		EditorGUI.indentLevel += 2;
		if (hasAudioHandlerValue && toggleGroupAudioTab)
			foreach (SoundCoroutine coroutine in audioHandler.SoundCoroutineCache.Values)
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
		EditorGUI.indentLevel -= 2;
	}
	private void AudioEditor()
	{

	}
}