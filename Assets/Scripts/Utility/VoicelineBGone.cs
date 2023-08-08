namespace B1NARY
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class VoicelineBGone : MonoBehaviour
	{
		public static bool Enabled { get => !PlayerConfig.Instance.voicelineBGone; set => PlayerConfig.Instance.voicelineBGone.Value = !value; }
		public AudioSource source;
		private void Reset()
		{
			source = GetComponent<AudioSource>();
		}
		public void OnEnable()
		{
			if (Enabled)
				return;
			source.Stop();
		}
	}
}
