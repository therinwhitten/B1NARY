using System.Globalization;
using UnityEngine;
using UnityEditor;

public sealed class AudioTab : DebuggerTab
{
	public override string Name => "Audio";

	private bool toggleGroupAudioTab = false;
	private Vector2 scrollPosAudioTab = Vector2.zero;
	public override void DisplayTab()
	{
		scrollPosAudioTab = EditorGUILayout.BeginScrollView(scrollPosAudioTab);
		bool hasValue = DebuggerWindow.TryGetter<AudioHandler>.TryGetObject(out var audioHandler);
		EditorGUILayout.LabelField($"Current Library: {(audioHandler.CustomAudioData == null ? "NaN" : audioHandler.CustomAudioData.name)}");
		toggleGroupAudioTab =
			EditorGUILayout.BeginFoldoutHeaderGroup(toggleGroupAudioTab, new GUIContent($"Sounds : {(hasValue ? audioHandler.SoundCoroutineCache.Count.ToString(CultureInfo.CurrentCulture) : "NaN")}"));
		EditorGUI.indentLevel += 2;
		if (hasValue && toggleGroupAudioTab)
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
				EditorGUILayout.Space();
				EditorGUI.indentLevel--;
			}
		EditorGUI.indentLevel -= 2;
		EditorGUILayout.EndFoldoutHeaderGroup();
		EditorGUILayout.EndScrollView();
	}
}