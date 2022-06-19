using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;


[CustomEditor(typeof(SoundLibrary))] //[CanEditMultipleObjects]
public class SoundLibraryEditor : Editor
{
	public void OnEnable()
	{
		var soundLibrary = (SoundLibrary)target;
		headerGroupsToggled = new List<bool>();
		for (int i = 0; i < soundLibrary.Length; i++)
			headerGroupsToggled.Add(false);
	}

	public override void OnInspectorGUI()
	{
		SoundLibrary soundLibrary = (SoundLibrary)target;
		AddTopButtons(soundLibrary);
		NullReferenceCheck(soundLibrary.customAudioClips);
		DisplayButtons(soundLibrary);
	}

	private void NullReferenceCheck(List<CustomAudioClip> customAudioClips)
	{
		if (customAudioClips.Select(x => x.clip).Where(x => x == null).Any())
			EditorGUILayout.HelpBox("The Sound Library contains empty parameters, which may cause issues!", MessageType.Error);
	}

	private void AddTopButtons(SoundLibrary soundLibrary)
	{
		var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24);
		rect.x += 2;
		rect.width -= 4;
		bool addSound = GUI.Button(rect, new GUIContent("Add New Sound"));
		if (addSound)
		{
			soundLibrary.customAudioClips.Add(new CustomAudioClip(null));
			headerGroupsToggled.Add(false);
		}
	}

	private List<bool> headerGroupsToggled;
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
			headerGroupsToggled[i] = EditorGUI.BeginFoldoutHeaderGroup(headerRect, headerGroupsToggled[i], name);
			var removeButtonRect = headerTitle;
			removeButtonRect.width /= 4;
			removeButtonRect.x += (removeButtonRect.width * 3) - 2;
			bool remove = GUI.Button(removeButtonRect, new GUIContent("Remove"));
			if (remove)
			{
				soundLibrary.customAudioClips.RemoveAt(i);
				headerGroupsToggled.RemoveAt(i);
				i--;
				continue;
			}
			if (headerGroupsToggled[i] == true)
			{
				EditorGUI.indentLevel++;
				var amogus = librarySerialized.FindProperty(nameof(SoundLibrary.customAudioClips)).GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(amogus.FindPropertyRelative(nameof(CustomAudioClip.clip)), new GUIContent("Selected Audioclip"));
				EditorGUILayout.PropertyField(amogus.FindPropertyRelative(nameof(CustomAudioClip.audioMixerGroup)), new GUIContent("Mixer Group"));
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].volume = EditorGUILayout.Slider(new GUIContent("Volume"), soundLibrary.customAudioClips[i].volume, 0, 1);
				soundLibrary.customAudioClips[i].volumeVariance = EditorGUILayout.Slider(new GUIContent("Volume Variance"), soundLibrary.customAudioClips[i].volumeVariance, 0, 1);
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].pitch = EditorGUILayout.Slider(new GUIContent("Pitch"), soundLibrary.customAudioClips[i].pitch, 0, 3);
				soundLibrary.customAudioClips[i].pitchVariance = EditorGUILayout.Slider(new GUIContent("Pitch Variance"), soundLibrary.customAudioClips[i].pitchVariance, 0, 1);
				EditorGUILayout.Space();
				soundLibrary.customAudioClips[i].loop = EditorGUILayout.Toggle(new GUIContent("Loopable"), soundLibrary.customAudioClips[i].loop);
				soundLibrary.customAudioClips[i].playOnAwake = EditorGUILayout.Toggle(new GUIContent("Play On Awake"), soundLibrary.customAudioClips[i].playOnAwake);
				soundLibrary.customAudioClips[i].randomType = (RandomFowarder.RandomType)EditorGUILayout.EnumPopup(new GUIContent("Random Variance Method"), soundLibrary.customAudioClips[i].randomType);
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(new GUIContent("Scene Transitioning"), EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				soundLibrary.customAudioClips[i].willFadeWhenTransitioning = EditorGUILayout.ToggleLeft(new GUIContent("Will Fade When Transitioning", "if the sound fade or stop during scene transition or not."), soundLibrary.customAudioClips[i].willFadeWhenTransitioning);
				if (soundLibrary.customAudioClips[i].willFadeWhenTransitioning)
					soundLibrary.customAudioClips[i].fadeWhenTransitioning = EditorGUILayout.Slider(new GUIContent("Fade When Transitioning", "How long it will take before completely fading into 0"), soundLibrary.customAudioClips[i].fadeWhenTransitioning, 0, 60);
				librarySerialized.ApplyModifiedProperties();
				EditorGUI.indentLevel -= 2;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}
	}
}