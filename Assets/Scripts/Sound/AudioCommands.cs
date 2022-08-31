namespace B1NARY.Audio
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public partial class AudioHandler
	{
		public static readonly IReadOnlyDictionary<string, Delegate> AudioDelegateCommands = new Dictionary<string, Delegate>()
		{
			// Although reflection is computationally heavy normally, it doesn't
			// - seem so in this situation.
			["fadeinsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeIn = float.Parse(floatStr);
				try { Instance.PlayFadedSound(name, fadeIn); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$": {name} is not a valid soundfile Path!\n" + ex);
				}
			}),
			["fadeoutsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeOut = float.Parse(floatStr);
				try { Instance.StopSoundViaFade(name, fadeOut); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$"{name} is not a valid soundfile Path!" + ex);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) + 
						$": Cannot find sound to close: {name}\n" + ex);
				}
			}),
			["playsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				try
				{
					Instance.PlaySound(name);
				}
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$"{name} is not a valid soundfile Path!" + ex);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$": Cannot find sound to play: {name}\n" + ex);
				}
			}),
			["stopsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				try { Instance.StopSound(name); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$"{name} is not a valid soundfile Path!" + ex);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning(nameof(AudioHandler) +
						$": Cannot find sound to play: {name}\n" + ex);
				}
			}),
		};
	}
}