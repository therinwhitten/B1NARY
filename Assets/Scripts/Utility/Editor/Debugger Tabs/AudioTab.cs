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
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Cannot show script when it is not in " +
					"play mode.", MessageType.Info);
				return;
			}
			if (!AudioController.HasInstance)
			{
				EditorGUILayout.HelpBox($"{nameof(AudioController)} does not exist!" +
					" Make sure to add it to your scene!", MessageType.Error);
				return;

			}
			var audioHandler = AudioController.Instance;
			EditorGUILayout.LabelField($"Current Library: {(audioHandler == null || audioHandler.CurrentSoundLibrary == null ? "Null" : audioHandler.CurrentSoundLibrary.name)}");
			AudioControllerEditor.DisplayAudioController(audioHandler);
		}
	}
}