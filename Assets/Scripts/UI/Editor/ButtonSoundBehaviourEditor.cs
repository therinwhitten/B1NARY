namespace B1NARY.UI.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System;
	using B1NARY.Editor;

	[CustomEditor(typeof(ButtonSoundBehaviour), true)]
	public class ButtonSoundBehaviourEditor : Editor
	{
		protected ButtonSoundBehaviour buttonSoundBehaviour;
		protected virtual void Awake()
		{
			buttonSoundBehaviour = (ButtonSoundBehaviour)target;
		}
		public override void OnInspectorGUI()
		{
			var configSerialized = buttonSoundBehaviour.config == null ? null : new SerializedObject(buttonSoundBehaviour.config);
			DirtyAuto.Property(serializedObject, nameof(ButtonSoundBehaviour.audioMixerGroup));
			DirtyAuto.Property(serializedObject, nameof(ButtonSoundBehaviour.config));
			if (configSerialized != null)
			{
				buttonSoundBehaviour.hoverIndex = ModifyIndex(buttonSoundBehaviour.hoverIndex, buttonSoundBehaviour.config.hoverSounds.Length, "Hover Index");
				DirtyAuto.Property(configSerialized, nameof(ButtonSoundConfig.hoverSounds));
				buttonSoundBehaviour.pressedIndex = ModifyIndex(buttonSoundBehaviour.pressedIndex, buttonSoundBehaviour.config.pressedSounds.Length, "Pressed Index");
				DirtyAuto.Property(configSerialized, nameof(ButtonSoundConfig.pressedSounds));
			}
		}

		protected virtual int ModifyIndex(int value, in int arrayLength, string label)
		{
			GUILayout.Space(4f);
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			bool isNegative = IsNegative(value);
			bool newNegativeValue = DirtyAuto.Toggle(buttonSoundBehaviour, new GUIContent("Random Value"), isNegative);
			if (isNegative != newNegativeValue)
				isNegative = IsNegative(value = isNegative ? 0 : -1);
			if (!isNegative && arrayLength > 1)
				value = DirtyAuto.Slider(buttonSoundBehaviour, new GUIContent("Index"), value, 0, arrayLength - 1);
			EditorGUI.indentLevel--;
			return value;

			bool IsNegative(int number) => number < 0;
		}
	}
}
