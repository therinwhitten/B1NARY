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
		[return: CommandToConsole]
		public static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("voiceline_b_gone", (args) =>
			{
				if (args.Length == 0)
				{
					HDConsole.WriteLine($"voiceline_b_gone {(PlayerConfig.Instance.voicelineBGone.Value == true ? '1' : '0')}");
					return;
				}
				bool output = byte.Parse(args[0]) == 1;
				if (output == PlayerConfig.Instance.voicelineBGone)
					return;
				PlayerConfig.Instance.voicelineBGone.Value = output;
				HDConsole.WriteLine(output ? $"Your ears will be spared." : "May the suffering continue.");
			}) { description = "Disables (when enabled) the voiceline at the start of the game.", optionalArguments = new string[] { "Toggle {0/1}" } },
		};

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
