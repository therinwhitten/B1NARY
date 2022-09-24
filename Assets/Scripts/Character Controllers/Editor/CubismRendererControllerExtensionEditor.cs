namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;
	using Live2D.Cubism.Editor.Inspectors;

	[CustomEditor(typeof(CubismRendererControllerExtension))]
	public class CubismRendererControllerExtensionEditor : CubismRenderControllerInspector
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var extension = (CubismRendererControllerExtension)target;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Extension", EditorStyles.boldLabel);
			EditorUtility.SetDirty(extension);
			extension.Color = EditorGUILayout.ColorField("Color", extension.Color);
			serializedObject.ApplyModifiedProperties();
		}
	}
}