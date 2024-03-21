namespace B1NARY
{
	using B1NARY.DataPersistence;
	using HDConsole;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class VoicelineBGone : MonoBehaviour
	{
		[return: CommandsFromGetter]
		public static HDCommand GetHDCommand() => HDCommand.AutoCompleteBool("bny_voiceline_b_gone", () => Enabled, (set) =>
		{
			if (Enabled == set)
				return;
			Enabled = set;
			HDConsole.WriteLine(set ? $"Your ears will be spared." : "May the suffering continue.");

		}, HDCommand.MainTags.None, "Disables (when enabled) the voiceline at the start of the game.");

		public static bool Enabled { get => !PlayerConfig.Instance.voicelineBGone; set => PlayerConfig.Instance.voicelineBGone.Value = !value; }
		public AudioSource source;
		private void Reset()
		{
			source = GetComponent<AudioSource>();
		}
		public void OnEnable()
		{
			if (!Enabled)
				return;
			source.Stop();
		}
	}
}
