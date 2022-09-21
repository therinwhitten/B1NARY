namespace B1NARY.Editor
{
	using UnityEditor;
	using B1NARY.Audio;

	[CustomEditor(typeof(SFXAudioController))]
	public class AudioHandlerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SFXAudioController audioHandler = (SFXAudioController)target;
			if (audioHandler.ActiveAudioTrackers != null)
				EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioHandler.ActiveAudioTrackers.Count}");
		}
	}
}