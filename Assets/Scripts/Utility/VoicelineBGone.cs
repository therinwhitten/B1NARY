namespace B1NARY
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class VoicelineBGone
	{
		public static bool Enabled { get => PlayerConfig.Instance.voicelineBGone; set => PlayerConfig.Instance.voicelineBGone.Value = value; }
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Start()
		{
			if (Enabled)
				return;
			GameObject obj = GameObject.Find("star default");
			if (obj == null)
				return;
			obj.GetComponent<AudioSource>().Stop();
		}
	}
}
