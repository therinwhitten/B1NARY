namespace B1NARY.Audio
{
	using B1NARY.DataPersistence;
	using B1NARY.DesignPatterns;
	using HDConsole;
	using OVSSerializer.Extras;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.Audio;

	internal class SoundSyncer : DesignPatterns.Singleton<SoundSyncer>
	{
		public const string MIXER_MASTER = "Master";
		public const string MIXER_MUSIC = "Music";
		public const string MIXER_SFX = "SFX";
		public const string MIXER_UI = "UI";
		public static Dictionary<string, ChangableValue<float>> GetConstantMixers() => new()
		{
			[MIXER_MASTER] = PlayerConfig.Instance.audio.master,
			[MIXER_MUSIC] = PlayerConfig.Instance.audio.music,
			[MIXER_SFX] = PlayerConfig.Instance.audio.SFX,
			[MIXER_UI] = PlayerConfig.Instance.audio.UI
		};
		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			HDCommand.AutoCompleteFloat("snd_master", () => PlayerConfig.Instance.audio.master, (set) => PlayerConfig.Instance.audio.master.Value = set, 0, 1, HDCommand.MainTags.None),
			HDCommand.AutoCompleteFloat("snd_music", () => PlayerConfig.Instance.audio.music, (set) => PlayerConfig.Instance.audio.music.Value = set, 0, 1, HDCommand.MainTags.None),
			HDCommand.AutoCompleteFloat("snd_sfx", () => PlayerConfig.Instance.audio.SFX, (set) => PlayerConfig.Instance.audio.SFX.Value = set, 0, 1, HDCommand.MainTags.None),
			HDCommand.AutoCompleteFloat("snd_ui", () => PlayerConfig.Instance.audio.UI, (set) => PlayerConfig.Instance.audio.UI.Value = set, 0, 1, HDCommand.MainTags.None),
			new HDCommand("snd_custom", new string[] { "sound name" }, new string[] { "0-1" }, (args) =>
			{
				string soundName = args[0];
				if (PlayerConfig.Instance.audio.characterVoices.TryGetValue(soundName, out float value) == false)
					value = 1f;
				if (args.Length <= 1)
				{
					HDConsole.WriteLine($"snd_custom {soundName} {value}");
					return;
				}
				PlayerConfig.Instance.audio.characterVoices[soundName] = float.Parse(args[1]);
			})
		};



		/// <summary>
		/// Sets both the volume ingame and in the data persistence.
		/// </summary>
		/// <param name="key"> The name of the volume. </param>
		/// <param name="value"> The Logarithmic(?) value. </param>
		public void SetVolume(string key, float value)
		{
			if (GetConstantMixers().TryGetValue(key, out var changableValue))
			{
				changableValue.Value = value;
				float output = Mathf.Log10(value) * 20;
				mixer.SetFloat(key, output);
			}
			else
			{
				float output = Mathf.Log10(value) * 20;
				if (!mixer.SetFloat(key, output))
				{
					Debug.LogWarning($"'{key}' is not found in the exposed parameters in mixer group, '{mixer.name}'", this);
					return;
				}
				PlayerConfig.Instance.audio.characterVoices[key] = value;
			}
		}

		public AudioMixer mixer;

		private void Start()
		{
			SetVolume(MIXER_MASTER, PlayerConfig.Instance.audio.master);
			SetVolume(MIXER_MUSIC, PlayerConfig.Instance.audio.music);
			SetVolume(MIXER_SFX, PlayerConfig.Instance.audio.SFX);
			SetVolume(MIXER_UI, PlayerConfig.Instance.audio.UI);
			// shut up loser
			using var enumerator = PlayerConfig.Instance.audio.characterVoices.GetEnumerator();
			while (enumerator.MoveNext())
				SetVolume(enumerator.Current.Key, enumerator.Current.Value);
		}
	}
}
