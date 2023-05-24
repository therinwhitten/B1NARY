/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using System.Linq;
using UnityEngine;


namespace Live2D.Cubism.Framework.MouthMovement
{
	/// <summary>
	/// Real-time <see cref="CubismMouthController"/> input from <see cref="AudioSource"/>s.
	/// </summary>
	[RequireComponent(typeof(CubismMouthController))]
	public sealed class CubismAudioMouthInput : MonoBehaviour
	{
		/// <summary>
		/// Audio source to sample.
		/// </summary>
		[SerializeField]
		public AudioSource AudioInput;


		/// <summary>
		/// Sampling quality.
		/// </summary>
		[SerializeField]
		public CubismAudioSamplingQuality SamplingQuality;


		/// <summary>
		/// Audio gain.
		/// </summary>
		[Range(1.0f, 10.0f)]
		public float Gain = 1.0f;

		/// <summary>
		/// Smoothing.
		/// </summary>
		[Range(0.0f, 1.0f)]
		public float Smoothing;


		/// <summary>
		/// Current samples.
		/// </summary>
		private float[] Samples { get; set; }

		/// <summary>
		/// Last root mean square.
		/// </summary>
		private float LastRms { get; set; }

		/// <summary>
		/// Buffer for <see cref="Mathf.SmoothDamp(float, float, ref float, float)"/> velocity.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		private float VelocityBuffer;

		/// <summary>
		/// Targeted <see cref="CubismMouthController"/>.
		/// </summary>
		private CubismMouthController Target { get; set; }
		public int TargetMouth { get; set; } = 0;


		/// <summary>
		/// True if instance is initialized.
		/// </summary>
		private bool IsInitialized
		{
			get { return Samples != null; }
		}


		/// <summary>
		/// Makes sure instance is initialized.
		/// </summary>
		private bool TryInitialize()
		{
			// Return early if already initialized.
			if (IsInitialized)
			{
				return false;
			}


			// Initialize samples buffer.
			switch (SamplingQuality)
			{
				default:
				case (CubismAudioSamplingQuality.VeryHigh):
				{
					Samples = new float[256];


					break;
				}
				case (CubismAudioSamplingQuality.Maximum):
				{
					Samples = new float[512];


					break;
				}
			}


			// Cache target.
			if (Target == null)
				Target = GetComponents<CubismMouthController>().Single(mouth => mouth.TargetMouth == TargetMouth);
			return true;
		}

		#region Unity Event Handling

		/// <summary>
		/// Samples audio input and applies it to mouth controller.
		/// </summary>
		private void Update()
		{
			// 'Fail' silently.
			if (AudioInput == null)
			{
				return;
			}


			// Sample audio.
			float total = 0f;


			AudioInput.GetOutputData(Samples, 0);


			for (var i = 0; i < Samples.Length; ++i)
			{
				var sample = Samples[i];


				total += (sample * sample);
			}


			// Compute root mean square over samples.
			var rms = Mathf.Sqrt(total / Samples.Length) * Gain;


			// Clamp root mean square.
			rms = Mathf.Clamp(rms, 0.0f, 1.0f);


			// Smooth rms.
			rms = Mathf.SmoothDamp(LastRms, rms, ref VelocityBuffer, Smoothing * 0.1f);


			// Set rms as mouth opening and store it for next evaluation.
			Target.MouthOpening = rms;


			LastRms = rms;
		}


		private void Reset()
		{
			// Prevent duplicates on component creation.
			int targetMouth = 0;
			CubismAudioMouthInput[] allControllers = GetComponents<CubismAudioMouthInput>();
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
		/// Initializes instance.
		/// </summary>
		private void OnEnable()
		{
			TryInitialize();
		}

		#endregion
	}
}

#if UNITY_EDITOR
namespace Live2D.Cubism.Framework.MouthMovement.Editor
{
	using Live2D.Cubism.Framework.Editor;
	using System;
	using System.Collections.Generic;
	using UnityEditor;
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
#endif