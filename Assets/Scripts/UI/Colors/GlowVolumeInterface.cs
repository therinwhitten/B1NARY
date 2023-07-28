namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Linq;
	using B1NARY.DesignPatterns;
	using UnityEngine;
	using UnityEngine.Rendering;
	using static UnityEngine.InputSystem.Controls.AxisControl;

	public sealed class GlowVolumeInterface : Singleton<GlowVolumeInterface>
	{
		public Volume volume;
		public float BloomIntensity
		{
			get => volume.profile.components.First().parameters[1].GetValue<float>();
			set
			{
				((VolumeParameter<float>)volume.profile.components.First().parameters[1]).value = value;
			}
		}

		private void Reset()
		{
			volume = GetComponent<Volume>();
		}

		protected override void SingletonAwake()
		{
			if (volume == null)
				throw new MissingFieldException(nameof(volume));
			PlayerConfig.Instance.graphics.glow.AttachValue(ChangedValue);
			//SceneManager.Instance.SwitchedScenes.AddPersistentListener(() =>
			//{
			//	if (volume == null)
			//		volume = GameObject.Find(nameToSearch).GetComponent<Volume>();
			//	BloomIntensity = PlayerConfig.Instance.graphics.glow.Value;
			//});
		}
		protected override void OnSingletonDestroy()
		{
			PlayerConfig.Instance.graphics.glow.ChangedValue -= ChangedValue;
		}

		private void ChangedValue(float newValue)
		{
			BloomIntensity = newValue;
		}
	}
}