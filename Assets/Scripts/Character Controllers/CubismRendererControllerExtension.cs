namespace B1NARY.CharacterManagement
{
	using Live2D.Cubism.Rendering;
	using UnityEngine;

	// Inheriting from this script causes some minor code modification, but should
	// - be good to go.
	public class CubismRendererControllerExtension : CubismRenderController
	{
		public Color Color
		{
			get => Renderers[0].Color; set
			{
				if (Renderers[0].Color == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].Color = value;
				}
			}
		}
		public Color MultiplyColor
		{
			get => Renderers[0].MultiplyColor; set
			{
				if (Renderers[0].MultiplyColor == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].MultiplyColor = value;
				}
			}
		}
		public Color ScreenColor
		{
			get => Renderers[0].ScreenColor; set
			{
				if (Renderers[0].ScreenColor == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].ScreenColor = value;
				}
			}
		}
		public Material Material
		{
			get => Renderers[0].Material; set
			{
				if (Renderers[0].Material == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].Material = value;
				}
			}
		}
	}
}
#if UNITY_EDITOR
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
#endif