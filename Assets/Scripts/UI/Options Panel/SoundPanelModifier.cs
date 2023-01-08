namespace B1NARY.Audio
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Audio;
	using UnityEngine.UI;

	public sealed class SoundPanelModifier : MonoBehaviour
	{
		public float GlobalVolume { get => AudioListener.volume; set => AudioListener.volume = value; }
		public bool UnityAudioToggle { get => AudioListener.pause; set => AudioListener.pause = value; }
		public AudioMixerGroup fullMixerGroup;

		public float SliderArgument { get; set; } = 0f;
		public void ChangedSliderValue(string namePath)
		{
			AudioMixer audioMixer = fullMixerGroup.audioMixer.FindMatchingGroups(namePath).Single().audioMixer;
			//var paths = new Queue<string>(fullPathData.Take(fullPathData.Count - 1));
			//AudioMixer currentMixer = fullMixerGroup.audioMixer;
			//while (paths.Count > 0)
			//	currentMixer = currentMixer.FindMatchingGroups(paths.Dequeue()).Single().audioMixer;
			//audioMixer.
			audioMixer.SetFloat("Volume", SliderArgument);
		}
	}
}

#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using B1NARY.Audio;
	using UnityEditor;

	[CustomEditor(typeof(SoundPanelModifier))]
	public sealed class SoundPanelModifierEditor : Editor
	{
		private SoundPanelModifier _modifier;
		private void Awake()
		{
			_modifier = (SoundPanelModifier)target;
		}
		private void OnEnable() => EditorUtility.SetDirty(_modifier);
		private void OnDisable()
		{
			if (_modifier != null)
				EditorUtility.ClearDirty(_modifier);
		}
		public override void OnInspectorGUI()
		{
			_modifier.UnityAudioToggle = EditorGUILayout.Toggle("Unity Audiolisteners", _modifier.UnityAudioToggle);
			_modifier.GlobalVolume = EditorGUILayout.Slider("Global Volume", _modifier.GlobalVolume, 0f, 1f);
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SoundPanelModifier.fullMixerGroup)));
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif