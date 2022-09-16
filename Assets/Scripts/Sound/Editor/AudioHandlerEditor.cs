namespace B1NARY.Editor
{
	using UnityEditor;
	using B1NARY.Audio;

	[CustomEditor(typeof(AudioHandler))]
	public class AudioHandlerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			AudioHandler audioHandler = (AudioHandler)target;
			if (audioHandler.SoundCoroutineCache != null)
				EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioHandler.SoundCoroutineCache.Count}");
		}
	}
}