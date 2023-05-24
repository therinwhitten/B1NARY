/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using Live2D.Cubism.Core;
using System.Linq;
using UnityEngine;


namespace Live2D.Cubism.Framework.MouthMovement
{
	/// <summary>
	/// Controls <see cref="CubismMouthParameter"/>s.
	/// </summary>
	public sealed class CubismMouthController : MonoBehaviour, ICubismUpdatable
	{
		/// <summary>
		/// The blend mode.
		/// </summary>
		[SerializeField]
		public CubismParameterBlendMode BlendMode = CubismParameterBlendMode.Multiply;


		/// <summary>
		/// The opening of the mouth.
		/// </summary>
		[SerializeField, Range(0f, 1f)]
		public float MouthOpening = 1f;


		/// <summary>
		/// Mouth parameters.
		/// </summary>
		private CubismParameter[] Destinations { get; set; }

		/// <summary>
		/// Model has update controller component.
		/// </summary>
		[HideInInspector]
		public bool HasUpdateController { get; set; }

		/// <summary>
		/// An index or tag that specifies which <see cref="CubismMouthParameter"/>s it should be
		/// assigned to. Defaulted to 0.
		/// </summary>
		[field: SerializeField]
		public int TargetMouth { get; set; } = 0;



		/// <summary>
		/// Refreshes controller. Call this method after adding and/or removing <see cref="CubismMouthParameter"/>s.
		/// </summary>
		public void Refresh()
		{
			CubismModel model = this.FindCubismModel();


			// Fail silently...
			if (model == null)
			{
				return;
			}


			// Cache destinations.
			CubismMouthParameter[] mouthTags = model
				.Parameters
				.GetComponentsMany<CubismMouthParameter>()
				// Gets tag or index of mouth
				.Where(mouthParam => mouthParam.mouthType == TargetMouth)
				.ToArray();


			Destinations = new CubismParameter[mouthTags.Length];


			for (var i = 0; i < mouthTags.Length; ++i)
				Destinations[i] = mouthTags[i].GetComponent<CubismParameter>();

			// Get cubism update controller.
			HasUpdateController = (GetComponent<CubismUpdateController>() != null);
		}

		/// <summary>
		/// Called by cubism update controller. Order to invoke OnLateUpdate.
		/// </summary>
		public int ExecutionOrder
		{
			get { return CubismUpdateExecutionOrder.CubismMouthController; }
		}

		/// <summary>
		/// Called by cubism update controller. Needs to invoke OnLateUpdate on Editing.
		/// </summary>
		public bool NeedsUpdateOnEditing
		{
			get { return false; }
		}

		/// <summary>
		/// Called by cubism update controller. Updates controller.
		/// </summary>
		/// <remarks>
		/// Make sure this method is called after any animations are evaluated.
		/// </remarks>
		public void OnLateUpdate()
		{
			// Fail silently.
			if (!enabled || Destinations == null)
			{
				return;
			}


			// Apply value.
			Destinations.BlendToValue(BlendMode, MouthOpening);
		}

		#region Unity Events Handling

		/// <summary>
		/// Called by Unity. Makes sure cache is initialized.
		/// </summary>
		private void Start()
		{
			// Initialize cache.
			Refresh();
		}

		/// <summary>
		/// Called by unity when creating a new component, or resetting to defaults
		/// in the editor.
		/// </summary>
		private void Reset()
		{
			// Prevent duplicates on component creation, based on the gameobject.
			int targetMouth = 0;
			CubismMouthController[] allControllers = GetComponents<CubismMouthController>();
			for (int i = 0; i < allControllers.Length; i++)
			{
				if (ReferenceEquals(this, allControllers[i]))
					continue;
				if (allControllers[i].TargetMouth != targetMouth)
					continue;
				targetMouth += 1;
				i = 0;
			}
			TargetMouth = targetMouth;
		}

		/// <summary>
		/// Called by Unity.
		/// </summary>
		private void LateUpdate()
		{
			if(!HasUpdateController)
			{
				OnLateUpdate();
			}
		}

		#endregion
	}
}
#if UNITY_EDITOR
namespace Live2D.Cubism.Framework.MouthMovement.Editor
{
	using Live2D.Cubism.Framework.Editor;
	using System;
	using UnityEditor;
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
#endif