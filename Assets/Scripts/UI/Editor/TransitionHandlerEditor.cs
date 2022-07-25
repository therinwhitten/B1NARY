using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(TransitionHandler))]
public class TransitionHandlerEditor : Editor
{
	private TransitionHandler m_Handler;
	public TransitionHandler TransitionHandler 
	{ 
		get
		{
			if (m_Handler == null)
				m_Handler = (TransitionHandler)target;
			return m_Handler;
		} 
	}

	public override void OnInspectorGUI()
	{
		SerializedObject serializedObject = new SerializedObject(TransitionHandler);
		serializedObject.Update();

		// Transitions
		EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionHandler.transitionShader)));
		if (TransitionHandler.transitionShader != null)
			TransitionHandler.fadePercentageName = EditorGUILayout
				.DelayedTextField("Fade Percentage Name", TransitionHandler.fadePercentageName);
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionHandler.textureIn)));
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionHandler.textureOut)));

		// Backgrounds
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Backgrounds", EditorStyles.boldLabel);
		TransitionHandler.backgroundCanvasName = EditorGUILayout.DelayedTextField("Background Canvas Name", TransitionHandler.backgroundCanvasName);
		TransitionHandler.AutomaticallyAssignAnimatedBG = EditorGUILayout.ToggleLeft("Automatically Assign Animated Background", TransitionHandler.AutomaticallyAssignAnimatedBG);

		serializedObject.ApplyModifiedProperties();
	}
}