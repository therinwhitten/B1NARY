namespace B1NARY.UI
{
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Rendering;

	public sealed class GlowVolumeInterface : MonoBehaviour
	{
		public Volume volume;
		public float BloomIntensity
		{
			get => volume.profile.components.Single().parameters[1].GetValue<float>();
			set
			{
				float clamped = PlayerConfig.Instance.graphics.glow = value;
				Debug.Log(clamped);
				((VolumeParameter<float>)volume.profile.components.Single().parameters[1]).value = clamped;
			}
		}

		private void Awake()
		{
			BloomIntensity = PlayerConfig.Instance.graphics.glow;
			SceneManager.Instance.SwitchedScenes.AddPersistentListener(() => BloomIntensity = PlayerConfig.Instance.graphics.glow);
		}
	}
}