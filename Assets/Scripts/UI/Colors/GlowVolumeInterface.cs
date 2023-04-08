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
				((VolumeParameter<float>)volume.profile.components.Single().parameters[1]).value = value;
			}
		}

		private void Awake()
		{
			nameToSearch = volume.name;
			PlayerConfig.Instance.graphics.glow.AttachValue((value) => BloomIntensity = value);
			SceneManager.Instance.SwitchedScenes.AddPersistentListener(() =>
			{
				if (volume == null)
					volume = GameObject.Find(nameToSearch).GetComponent<Volume>();
				BloomIntensity = PlayerConfig.Instance.graphics.glow.Value;
			});
		}
	}
}