namespace B1NARY.Audio
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	///	<summary>
	///		A group of sounds customized by the user, can be used separately 
	///		or multiple.
	///	</summary>
	[CreateAssetMenu(fileName = "New Sound Library", menuName = "B1NARY/Sound Library (SL)", order = 0)]
	public class SoundLibrary : ScriptableObject
	{
		/// <summary>
		/// Gets a value indicating whether it has sounds that has an imageOption that can
		/// play it on awake.
		/// </summary>
		public bool ContainsPlayOnAwakeCommands { get; private set; }
		/// <summary>
		/// The total list/copy that are wanted to play on awake. set as <see langword="null"/> 
		/// if <see cref="ContainsPlayOnAwakeCommands"/> is <see langword="false"/>
		/// </summary>
		public IEnumerable<CustomAudioClip> PlayOnAwakeCommands { get; private set; } = null;

		/// <summary>
		/// The list of <see cref="CustomAudioClip"/>s that the <see cref="SoundLibrary"/>
		/// stores. This is inputted by the user.
		/// </summary>
		public List<CustomAudioClip> customAudioClips;

		/// <summary>
		/// Gets the <see cref="CustomAudioClip"/> at the specified index. Basically
		/// forwards to the list.
		/// </summary>
		public CustomAudioClip this[int index] => customAudioClips[index];

		/// <summary>
		/// Gets the count of <see cref="customAudioClips"/>.
		/// </summary>
		public int Count => customAudioClips.Count;

		private void Awake()
		{
			if (customAudioClips == null)
			{
				customAudioClips = new List<CustomAudioClip>();
				Debug.LogError("Clip data has been found null. Replacing " +
					"default settings. Hopefully you made a backup of them?", this);
				return;
			}
			IEnumerable<CustomAudioClip> playAwakeClips = customAudioClips.Where(CClip => CClip.playOnAwake);
			if (ContainsPlayOnAwakeCommands = playAwakeClips.Any())
				PlayOnAwakeCommands = playAwakeClips;
		}
		public bool TryGetCustomAudioClip(string name, out CustomAudioClip clip)
		{
			for (int i = 0; i < customAudioClips.Count; i++)
				if (customAudioClips[i].Name == name)
				{
					clip = customAudioClips[i];
					return true;
				}
			clip = null;
			return false;
		}
		public bool TryGetCustomAudioClip(AudioClip example, out CustomAudioClip clip)
		{
			for (int i = 0; i < customAudioClips.Count; i++)
				if (customAudioClips[i].clip == example)
				{
					clip = customAudioClips[i];
					return true;
				}
			clip = null;
			return false;
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.Audio.Editor
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using B1NARY.Audio;

	[CustomEditor(typeof(SoundLibrary))]
	public class SoundLibraryEditor : Editor
	{
		static Dictionary<string, List<bool>> headerGroupsToggledForMultiple = new();
		string Name
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
			EditorUtility.SetDirty(target);
			if (!headerGroupsToggledForMultiple.ContainsKey(Name))
				headerGroupsToggledForMultiple.Add(Name,
					Enumerable.Repeat(false, soundLibrary.Count).ToList());
		}
		public void OnDisable()
		{
			EditorUtility.ClearDirty(target);
		}

		public override void OnInspectorGUI()
		{
			SoundLibrary soundLibrary = (SoundLibrary)target;
			AddTopButtons(soundLibrary);
			DisplayButtons(soundLibrary);
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
					soundLibrary.customAudioClips[i].volumeVariance = EditorGUILayout.Slider(new GUIContent("Volume Variance", "Starting from the highest point of the volume can get, it decreases the sound from the percentage of this slider and rolls a random value from there."), soundLibrary.customAudioClips[i].volumeVariance, 0, 1);
					EditorGUILayout.Space();
					soundLibrary.customAudioClips[i].pitch = EditorGUILayout.Slider(new GUIContent("Pitch", "Percentage of pitch from 0 to 3, starting from 1, the audioClip plays in when called"), soundLibrary.customAudioClips[i].pitch, 0, 3);
					soundLibrary.customAudioClips[i].pitchVariance = EditorGUILayout.Slider(new GUIContent("Pitch Variance", "Starting from the highest point of the pitch can get, it decreases the sound from the percentage of this slider and rolls a random value from there."), soundLibrary.customAudioClips[i].pitchVariance, 0, 1);
					EditorGUILayout.Space();
					soundLibrary.customAudioClips[i].loop = EditorGUILayout.Toggle(new GUIContent("Loopable", "If the audioclip finishes, it will play again instead of stopping."), soundLibrary.customAudioClips[i].loop);
					soundLibrary.customAudioClips[i].playOnAwake = EditorGUILayout.Toggle(new GUIContent("Play On Scene Start", "Plays the audioClip on the start of the scene."), soundLibrary.customAudioClips[i].playOnAwake);
					soundLibrary.customAudioClips[i].randomType = (RandomForwarder.RandomType)EditorGUILayout.EnumPopup(new GUIContent("Random Variance Method", "Use which type of random number generator."), soundLibrary.customAudioClips[i].randomType);
					EditorGUILayout.Space();
					EditorGUILayout.LabelField(new GUIContent("Scene Transitioning"), EditorStyles.boldLabel);
					EditorGUI.indentLevel++;
					soundLibrary.customAudioClips[i].destroyWhenTransitioningScenes = EditorGUILayout.ToggleLeft(new GUIContent("Destroy Sound When Transitioning", "When the scene transitions, the currently selected Sound Library will be looked at. When this is enabled, the Sound will be removed automatically. Otherwise, not."), soundLibrary.customAudioClips[i].destroyWhenTransitioningScenes);
					if (soundLibrary.customAudioClips[i].destroyWhenTransitioningScenes)
						soundLibrary.customAudioClips[i].fadeTime = EditorGUILayout.Slider(new GUIContent("Fade Time For Scene Transition", "The fade time when the scene transitions. Happens when its fully finished transitioning scenes."), soundLibrary.customAudioClips[i].fadeTime, 0, 60);
					librarySerialized.ApplyModifiedProperties();
					EditorGUI.indentLevel -= 2;
					EditorGUILayout.Space();
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
				if (soundLibrary.customAudioClips[i].clip == null)
					EditorGUILayout.HelpBox("This entry contains empty parameters, which will cause issues! Delete or Define them.", MessageType.Error);
			}

		}
	}
}
#endif