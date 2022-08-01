namespace B1NARY.Editor
{
	using UnityEditor;
	using B1NARY.Sounds;

	[CustomEditor(typeof(AudioHandler))]
	public class AudioHandlerEditor : Editor
	{
		private bool showVA = false;
		public override void OnInspectorGUI()
		{
			AudioHandler audioHandler = (AudioHandler)target;
			bool hasVoiceActor = audioHandler.VoiceActorHandler != null;
			if (showVA = EditorGUILayout.BeginFoldoutHeaderGroup(showVA, $"Contains Voice Actor: {hasVoiceActor}") && hasVoiceActor)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField(audioHandler.VoiceActorHandler.audioSource.ToString());
				EditorGUI.indentLevel--;
			}
			if (audioHandler.SoundCoroutineCache != null)
				EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioHandler.SoundCoroutineCache.Count}");
		}
	}
}