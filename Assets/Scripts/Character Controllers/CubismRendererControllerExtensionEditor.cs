namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;
	using B1NARY.CharacterManagement;
	using Live2D.Cubism.Editor.Inspectors;

	[CustomEditor(typeof(CubismRendererControllerExtension))]
	public class CubismRendererControllerExtensionEditor : CubismRenderControllerInspector
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var extension = (CubismRendererControllerExtension)target;
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(new GUIContent("B1NARY's Extensions", 
				"Because this is being edited by an outside source, this will" +
				" derive off of the base cubism controller. For any devs out" +
				" there, feel free to Ctrl+C & Ctrl+V!"), EditorStyles.boldLabel);
			extension.Color = DirtyAuto.Field(extension, new GUIContent("Color", "Uses the first drawable as a reference, and modifies them accordingly."), extension.Color);
			extension.MultiplyColor = DirtyAuto.Field(extension, new GUIContent("Multiply Color", "Uses the first drawable as a reference, and modifies them accordingly."), extension.MultiplyColor);
			extension.ScreenColor = DirtyAuto.Field(extension, new GUIContent("Screen Color", "Uses the first drawable as a reference, and modifies them accordingly."), extension.ScreenColor);
			extension.Material = DirtyAuto.Field(extension, new GUIContent("Material", "Uses the first drawable as a reference, and modifies them accordingly."), extension.Material, false);
		}
	}
}