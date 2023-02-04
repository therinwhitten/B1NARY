namespace B1NARY.UI
{
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Rendering;

	public sealed class GlowVolumeInterface : MonoBehaviour
	{
		private string nameToSearch;
		public Volume volume;
		public float BloomIntensity
		{
			get => volume.profile.components.Single().parameters[1].GetValue<float>();
			set
			{
				float clamped = PlayerConfig.Instance.graphics.glow = value;
				((VolumeParameter<float>)volume.profile.components.Single().parameters[1]).value = clamped;
			}
		}

		private void Awake()
		{
			nameToSearch = volume.name;
			BloomIntensity = PlayerConfig.Instance.graphics.glow;
			SceneManager.Instance.SwitchedScenes.AddPersistentListener(() =>
			{
				if (volume == null)
					volume = GameObject.Find(nameToSearch).GetComponent<Volume>();
				BloomIntensity = PlayerConfig.Instance.graphics.glow;
			});
		}
	}
}