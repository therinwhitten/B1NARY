#if UNITY_EDITOR
namespace B1NARY.Editor.Debugger
{
	using System.Globalization;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.Audio;
	using B1NARY.Audio.Editor;

	public sealed class AudioTab : DebuggerTab
	{
		public override GUIContent Name => new("Audio");
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
			EditorGUILayout.LabelField($"Current Library: {(audioHandler == null || audioHandler.ActiveLibrary == null ? "Null" : audioHandler.ActiveLibrary.name)}");
			AudioControllerEditor.DisplayAudioController(audioHandler);
		}
	}
}
#endif