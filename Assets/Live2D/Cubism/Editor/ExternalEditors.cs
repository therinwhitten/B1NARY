namespace Live2D.Cubism.Framework.MouthMovement.Editor
{
	using Live2D.Cubism.Framework.Editor;
	using System;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Rendering;

	[CustomEditor(typeof(CubismMouthController))]
	public class CubismMouthControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			CubismMouthController controller = (CubismMouthController)target;

			controller.BlendMode = DirtyAuto.Popup(controller, new GUIContent("Blend Mode"), controller.BlendMode);
			controller.MouthOpening = DirtyAuto.Slider(controller, new GUIContent("Mouth Opening"), controller.MouthOpening, 0f, 1f);

			CubismMouthController[] otherControllers = controller.gameObject.GetComponents<CubismMouthController>();
			if (otherControllers.Length > 1)
			{
				controller.TargetMouth = DirtyAuto.Field(controller, new GUIContent("Target Mouth"), controller.TargetMouth);
				for (int i = 0; i < otherControllers.Length; i++)
					if (!ReferenceEquals(controller, otherControllers[i]) && otherControllers[i].TargetMouth == controller.TargetMouth)
						EditorGUILayout.HelpBox($"{controller.TargetMouth} matches other components of '{nameof(CubismMouthController)}' and may cause errors!", MessageType.Warning);
			}
			else if (controller.TargetMouth != 0)
			{
				controller.TargetMouth = 0;
				controller.SetDirty();
			}
		}
	}
}
namespace Live2D.Cubism.Framework.MouthMovement.Editor
{
	using Live2D.Cubism.Framework.Editor;
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Rendering;

	[CustomEditor(typeof(CubismAudioMouthInput))]
	public class CubismAudioMouthInputEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			CubismAudioMouthInput controller = (CubismAudioMouthInput)target;

			controller.AudioInput = DirtyAuto.Field(controller, new GUIContent("Blend Mode"), controller.AudioInput, true);
			controller.SamplingQuality = DirtyAuto.Popup(controller, new GUIContent("Mouth Opening"), controller.SamplingQuality);
			controller.Gain = DirtyAuto.Slider(controller, new GUIContent("Gain"), controller.Gain, 1f, 10f);
			controller.Smoothing = DirtyAuto.Slider(controller, new GUIContent("Smoothing"), controller.Smoothing, 0f, 1f);

			CubismAudioMouthInput[] otherControllers = controller.gameObject.GetComponents<CubismAudioMouthInput>();
			if (otherControllers.Length > 1)
			{
				controller.TargetMouth = DirtyAuto.Field(controller, new GUIContent("Target Mouth"), controller.TargetMouth);
				for (int i = 0; i < otherControllers.Length; i++)
					if (!ReferenceEquals(controller, otherControllers[i]) && otherControllers[i].TargetMouth == controller.TargetMouth)
						EditorGUILayout.HelpBox($"'{controller.TargetMouth}' matches other components of '{nameof(CubismAudioMouthInput)}' and may cause errors!", MessageType.Warning);
			}
			else if (controller.TargetMouth != 0)
			{
				controller.TargetMouth = 0;
				controller.SetDirty();
			}
		}
	}
}