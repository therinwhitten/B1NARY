/* I would like to make the audiohandler more user friendly, but unity sucks
 * - so much with serialization so much I don't want to deal with the issues.
 * - Why can't i serialize properties with a property and only fields?

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioHandler))] 
public class AudioHandlerEditor : Editor
{
	private AudioHandler audioHandler;
	private SerializedProperty serializedCustomAudioData;
	private void OnEnable()
	{
		audioHandler = (AudioHandler)target;
		serializedCustomAudioData = 
			serializedObject.FindProperty(nameof(AudioHandler.CustomAudioData));
	}

	private bool LoadAudioSource
	{
		get => audioHandler.loadAudioSourceForVoiceActors;
		set => audioHandler.loadAudioSourceForVoiceActors = value;
	}
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		serializedObject.Update();
		LoadAudioSource = 
			EditorGUILayout.Toggle(new GUIContent("Load Audio Source For Voice Actors"), LoadAudioSource);
		EditorGUILayout.PropertyField(serializedCustomAudioData);
		// Logging purposes
	}
}
*/