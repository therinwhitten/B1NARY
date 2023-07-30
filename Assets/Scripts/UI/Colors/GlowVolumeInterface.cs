namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Rendering;
	using static UnityEngine.InputSystem.Controls.AxisControl;

	public sealed class GlowVolumeInterface : Singleton<GlowVolumeInterface>
	{
		public Volume volume;
		public int targetComponent;
		public int optionIndex;
		public float BloomIntensity
		{
			get => volume.profile.components[targetComponent].parameters[optionIndex].GetValue<float>();
			set
			{
				((VolumeParameter<float>)volume.profile.components.First().parameters[optionIndex]).value = value;
			}
		}

		private void Reset()
		{
			volume = GetComponent<Volume>();
			if (volume != null && volume.profile != null)
				for (int i = 0; i < volume.profile.components.Count; i++)
				{
					VolumeComponent component = volume.profile.components[i];
					if (component.name.StartsWith("Bloom"))
					{
						VolumeComponent targetComponent = volume.profile.components[i];
						this.targetComponent = i;
						this.optionIndex = 2;
						break;
					}
				}
		}

		protected override void SingletonAwake()
		{
			PlayerConfig.Instance.graphics.glow.AttachValue(ModifiedValue);
			//SceneManager.Instance.SwitchedScenes.AddPersistentListener(() =>
			//{
			//	if (volume == null)
			//		volume = GameObject.Find(nameToSearch).GetComponent<Volume>();
			//	BloomIntensity = PlayerConfig.Instance.graphics.glow.Value;
			//});
		}
		private void ModifiedValue(float newValue)
		{
			BloomIntensity = newValue;
		}
		protected override void OnSingletonDestroy()
		{
			PlayerConfig.Instance.graphics.glow.ValueChanged -= ModifiedValue;
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using System;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(GlowVolumeInterface))]
	public class GlowVolumeEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GlowVolumeInterface @interface = (GlowVolumeInterface)target;
			@interface.BloomIntensity = EditorGUILayout.IntSlider((int)@interface.BloomIntensity, 0, 10);
		}
	}
}
#endif