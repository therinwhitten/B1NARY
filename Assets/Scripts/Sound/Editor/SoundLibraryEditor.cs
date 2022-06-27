using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SoundLibrary))]
public class SoundLibraryEditor : Editor
{
	static Dictionary<string, List<bool>> headerGroupsToggledForMultiple 
		= new Dictionary<string, List<bool>>();
	public string Name
	{
		get
		{
			string assetPath = AssetDatabase.GetAssetPath(target.GetInstanceID());
			return Path.GetFileNameWithoutExtension(assetPath);
		}
	}



	public void OnEnable()
	{
		SoundLibrary soundLibrary = (SoundLibrary)target;
		EditorUtility.SetDirty(soundLibrary);
		if (!headerGroupsToggledForMultiple.ContainsKey(Name))
			headerGroupsToggledForMultiple.Add(Name, 
				Enumerable.Repeat(false, soundLibrary.Length).ToList());
	}

	public override void OnInspectorGUI()
	{
		SoundLibrary soundLibrary = (SoundLibrary)target;
		AddTopButtons(soundLibrary);
		NullReferenceCheck(soundLibrary.customAudioClips);
		DisplayButtons(soundLibrary);
	}

	private void NullReferenceCheck(IEnumerable<CustomAudioClip> customAudioClips)
	{
		if (customAudioClips == null)
			return;
		if (customAudioClips.Any(CustomClip => CustomClip.clip == null))
			EditorGUILayout.HelpBox("The Sound Library contains empty parameters, which may cause issues!", MessageType.Error);
	}

	private void AddTopButtons(SoundLibrary soundLibrary)
	{
		var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24);
		rect.x += 2;
		rect.width -= 4;
		bool addSound = GUI.Button(rect, new GUIContent("Add New Sound", "Adds a new empty sound slot to the bottom of the list."));
		if (addSound)
		{
			soundLibrary.customAudioClips.Add(new CustomAudioClip(null));
			headerGroupsToggledForMultiple[Name].Add(false);
		}
	}

	private void DisplayButtons(SoundLibrary soundLibrary)
	{
		var librarySerialized = new SerializedObject(soundLibrary);
		librarySerialized.Update();
		for (int i = 0; i < soundLibrary.customAudioClips.Count; i++)
		{
			string name = soundLibrary.customAudioClips[i].clip != null ?
				soundLibrary.customAudioClips[i].clip.name :
				"! Empty Sound File !";
			Rect headerTitle = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
			var headerRect = headerTitle;
			headerRect.width = headerRect.width / 4 * 3;
			headerGroupsToggledForMultiple[Name][i] = EditorGUI.BeginFoldoutHeaderGroup(headerRect, headerGroupsToggledForMultiple[Name][i], name);
			var removeButtonRect = headerTitle;
			removeButtonRect.width /= 4;
			removeButtonRect.x += (removeButtonRect.width * 3) + 4;
			removeButtonRect.width -= 6;
			if (GUI.Button(removeButtonRect, new GUIContent("Remove", "Remove the sound from the library.")))
			{
				soundLibrary.customAudioClips.RemoveAt(i);
				headerGroupsToggledForMultiple[Name].RemoveAt(i);
				i--;
				continue;
			}
			if (headerGroupsToggledForMultiple[Name][i] == true)
			{
				EditorGUI.indentLevel++;
				SerializedProperty customAudioClip = librarySerialized.FindProperty(nameof(SoundLibrary.customAudioClips)).GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(customAudioClip.FindPropertyRelative(nameof(CustomAudioClip.clip)), new GUIContent("Selected Audioclip", "An audioclip. The name of the audio clip gets called from the AudioHandler instead of having a custom name."));
				EditorGUILayout.PropertyField(customAudioClip.FindPropertyRelative(nameof(CustomAudioClip.audioMixerGroup)), new GUIContent("Mixer Group", "An Audio Mixer Group, meant to assign certain sounds to be played differently based on a general mixer group."));
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].volume = EditorGUILayout.Slider(new GUIContent("Volume", "Percentage of volume from 0 to 1 the audioClip plays in when called"), soundLibrary.customAudioClips[i].volume, 0, 1);
				EditorGUILayout.MinMaxSlider(new GUIContent("Volume Variance", "Uses a random value to calculate the volume every time its played and when there isn't the sound playing already."), ref soundLibrary.customAudioClips[i].minVolumeVariance, ref soundLibrary.customAudioClips[i].maxVolumeVariance, 0, 1);
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].pitch = EditorGUILayout.Slider(new GUIContent("Pitch", "Percentage of pitch from 0 to 3, starting from 1, the audioClip plays in when called"), soundLibrary.customAudioClips[i].pitch, 0, 3);
				EditorGUILayout.MinMaxSlider(new GUIContent("Pitch Variance", "Uses a random value to calculate the pitch every time its played and when there isn't the sound playing already."), ref soundLibrary.customAudioClips[i].minPitchVariance, ref soundLibrary.customAudioClips[i].maxPitchVariance, 0, 1);
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].loop = EditorGUILayout.Toggle(new GUIContent("Loopable", "If the audioclip finishes, it will play again instead of stopping."), soundLibrary.customAudioClips[i].loop);
				soundLibrary.customAudioClips[i].playOnAwake = EditorGUILayout.Toggle(new GUIContent("Play On Scene Start", "Plays the audioClip on the start of the scene."), soundLibrary.customAudioClips[i].playOnAwake);
				soundLibrary.customAudioClips[i].randomType = (RandomFowarder.RandomType)EditorGUILayout.EnumPopup(new GUIContent("Random Variance Method", "Use which type of random number generator, keep in mind Doom's random num gen doesn't work well with volume and pitch."), soundLibrary.customAudioClips[i].randomType);
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(new GUIContent("Scene Transitioning"), EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				soundLibrary.customAudioClips[i].willFadeWhenTransitioning = EditorGUILayout.ToggleLeft(new GUIContent("Will Fade When Transitioning", "if the sound fade or stop during scene transition or not."), soundLibrary.customAudioClips[i].willFadeWhenTransitioning);
				if (soundLibrary.customAudioClips[i].willFadeWhenTransitioning)
					soundLibrary.customAudioClips[i].fadeWhenTransitioning = EditorGUILayout.Slider(new GUIContent("Fade When Transitioning", "How long it will take before it deletes itself."), soundLibrary.customAudioClips[i].fadeWhenTransitioning, 0, 60);
				librarySerialized.ApplyModifiedProperties();
				EditorGUI.indentLevel -= 2;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}
		
	}
}