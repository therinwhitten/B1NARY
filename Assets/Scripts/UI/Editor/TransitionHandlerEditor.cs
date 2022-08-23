namespace B1NARY.Editor
{
	using System;
	using UnityEditor;

	[CustomEditor(typeof(TransitionManager)), Obsolete]
	public class TransitionHandlerEditor : Editor
	{
		private TransitionManager m_Handler;
		public TransitionManager TransitionHandler
		{
			get
			{
				if (m_Handler == null)
					m_Handler = (TransitionManager)target;
				return m_Handler;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			/*
			SerializedObject serializedObject = new SerializedObject(TransitionManager);
			serializedObject.Update();

			// Transitions
			EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.transitionShader)));
			TransitionManager.useTransitionValue = EditorGUILayout.Toggle("Use Transition Value", TransitionManager.useTransitionValue);
			if (TransitionManager.transitionShader != null && TransitionManager.useTransitionValue)
				TransitionManager.fadePercentageName = EditorGUILayout
					.DelayedTextField("Fade Percentage Name", TransitionManager.fadePercentageName);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.textureIn)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TransitionManager.textureOut)));

			// Backgrounds
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Backgrounds", EditorStyles.boldLabel);
			TransitionManager.backgroundCanvasName = EditorGUILayout.DelayedTextField("Background Canvas Name", TransitionManager.backgroundCanvasName);
			TransitionManager.AutomaticallyAssignAnimatedBG = EditorGUILayout.ToggleLeft("Automatically Assign Animated Background", TransitionManager.AutomaticallyAssignAnimatedBG);

			serializedObject.ApplyModifiedProperties();
			*/
		}
	}
}