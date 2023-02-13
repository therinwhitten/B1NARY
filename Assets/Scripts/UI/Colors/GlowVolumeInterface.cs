namespace B1NARY.UI
{
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Rendering;
	using static UnityEngine.InputSystem.Controls.AxisControl;

	public sealed class GlowVolumeInterface : MonoBehaviour
	{
		private string nameToSearch;
		public Volume volume;
		public float BloomIntensity
		{
			get => volume.profile.components.Single().parameters[1].GetValue<float>();
			set
			{
				float clamped = B1NARYConfig.Graphics.Glow = value;
				UpdateProfile(clamped);
			}
		}
		private void UpdateProfile(float clamped) => ((VolumeParameter<float>)volume.profile.components.Single().parameters[1]).value = clamped;

		private void Awake()
		{
			nameToSearch = volume.name;
			UpdateProfile(B1NARYConfig.Graphics.Glow);
			SceneManager.Instance.SwitchedScenes.AddPersistentListener(() =>
			{
				if (volume == null)
					volume = GameObject.Find(nameToSearch).GetComponent<Volume>();
				UpdateProfile(B1NARYConfig.Graphics.Glow);
			});
		}
	}
}