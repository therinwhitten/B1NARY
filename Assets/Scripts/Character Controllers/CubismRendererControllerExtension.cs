namespace B1NARY.CharacterManagement
{
	using Live2D.Cubism.Rendering;
	using UnityEngine;

	public static class CubismRendererControllerExtension
	{
		public static Color GetColor(this CubismRenderController cubismRenderController)
		{
			return cubismRenderController.Renderers[0].Color;
		}
		public static void SetColor(this CubismRenderController cubismRenderController, Color value)
		{
			if (cubismRenderController.Renderers[0].Color == value)
				return;
			for (int i = 0; i < cubismRenderController.Renderers.Length; i++)
			{
				cubismRenderController.Renderers[i].Color = value;
			}
		}

		public static Color GetMultiplyColor(this CubismRenderController cubismRenderController)
		{
			return cubismRenderController.Renderers[0].MultiplyColor;
		}
		public static void SetMultiplyColor(this CubismRenderController cubismRenderController, Color value)
		{
			if (cubismRenderController.Renderers[0].MultiplyColor == value)
				return;
			for (int i = 0; i < cubismRenderController.Renderers.Length; i++)
			{
				cubismRenderController.Renderers[i].MultiplyColor = value;
			}
		}

		public static Color GetScreenColor(this CubismRenderController cubismRenderController)
		{
			return cubismRenderController.Renderers[0].ScreenColor;
		}
		public static void SetScreenColor(this CubismRenderController cubismRenderController, Color value)
		{
			if (cubismRenderController.Renderers[0].ScreenColor == value)
				return;
			for (int i = 0; i < cubismRenderController.Renderers.Length; i++)
			{
				cubismRenderController.Renderers[i].ScreenColor = value;
			}
		}

		public static Material GetMaterial(this CubismRenderController cubismRenderController)
		{
			return cubismRenderController.Renderers[0].Material;
		}
		public static void SetMaterial(this CubismRenderController cubismRenderController, Material value)
		{
			if (cubismRenderController.Renderers[0].Material == value)
				return;
			for (int i = 0; i < cubismRenderController.Renderers.Length; i++)
			{
				cubismRenderController.Renderers[i].Material = value;
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
	using Live2D.Cubism.Rendering;

	[CustomEditor(typeof(CubismRenderController))]
	public class B1NARYLive2DControllerEditor : Editor
	{
		private CubismRenderController controller;
		private void Awake()
		{
			controller = (CubismRenderController)target;
		}

		private bool SortingGroup = false,
			AdvancedGroup = false;

		public override void OnInspectorGUI()
		{
			controller.Opacity = DirtyAuto.Slider(target, new GUIContent("Opacity"), controller.Opacity, 0f, 1f);
			try
			{
				controller.SetColor(DirtyAuto.Field(target, new GUIContent("Color", "Uses the first drawable as a reference, and modifies them accordingly."), controller.GetColor()));
				controller.SetMultiplyColor(DirtyAuto.Field(target, new GUIContent("Multiply Color", "Uses the first drawable as a reference, and modifies them accordingly."), controller.GetMultiplyColor()));
				controller.SetScreenColor(DirtyAuto.Field(target, new GUIContent("Screen Color", "Uses the first drawable as a reference, and modifies them accordingly."), controller.GetScreenColor()));
				controller.SetMaterial(DirtyAuto.Field(target, new GUIContent("Material", "Uses the first drawable as a reference, and modifies them accordingly."), controller.GetMaterial(), false));
			}
			catch { }
			controller.OverwriteFlagForModelMultiplyColors = DirtyAuto.ToggleLeft(target, new GUIContent("Overwrite Flag For Model Multiply Colors"), controller.OverwriteFlagForModelMultiplyColors);
			controller.OverwriteFlagForModelScreenColors = DirtyAuto.ToggleLeft(target, new GUIContent("Overwrite Flag For Model Screen Colors"), controller.OverwriteFlagForModelScreenColors);
			if (SortingGroup = EditorGUILayout.Foldout(SortingGroup, "Sorting", EditorStyles.foldoutHeader))
			{
				EditorGUI.indentLevel++;
				controller.SortingLayer = DirtyAuto.Field(target, new GUIContent("Layer"), controller.SortingLayer);
				controller.SortingOrder = DirtyAuto.Field(target, new GUIContent("In Order"), controller.SortingOrder);
				controller.SortingMode = DirtyAuto.Popup(target, new GUIContent("Mode"), controller.SortingMode);
				EditorGUI.indentLevel--;
			}
			if (AdvancedGroup = EditorGUILayout.Foldout(AdvancedGroup, "Advanced", EditorStyles.foldoutHeader))
			{
				EditorGUI.indentLevel++;
				controller.CameraToFace = DirtyAuto.Field(target, new GUIContent("Camera To Face"), controller.CameraToFace, true);
				controller.OpacityHandler = DirtyAuto.Field(target, new GUIContent("Opacity Handler"), controller.OpacityHandler, true);
				controller.DrawOrderHandler = DirtyAuto.Field(target, new GUIContent("Draw Order Handler"), controller.DrawOrderHandler, true);
				controller.MultiplyColorHandler = DirtyAuto.Field(target, new GUIContent("Multiply Color Handler"), controller.MultiplyColorHandler, true);
				controller.ScreenColorHandler = DirtyAuto.Field(target, new GUIContent("Screen Color Handler"), controller.ScreenColorHandler, true);
				EditorGUI.indentLevel--;
			}
			if (!controller.gameObject.TryGetComponent<Live2DActor>(out _))
				if (GUILayout.Button("Add Live2D Character Controller"))
				{
					controller.gameObject.AddComponent<Live2DActor>();
					target.SetDirty();
				}
		}
	}
}
#endif