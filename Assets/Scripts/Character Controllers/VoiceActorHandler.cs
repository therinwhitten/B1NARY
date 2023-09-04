namespace B1NARY.CharacterManagement
{
	using System;
	using UnityEngine;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.Audio;
	using Live2D.Cubism.Framework.MouthMovement;
	using UnityEngine.Audio;
	using System.IO;

	public class VoiceActorHandler : MonoBehaviour, IAudioInfo
	{
		public static CommandArray Commands = new()
		{
			["switchvoice"] = ((Action<string>)((intRaw) =>
			{
				int mouthIndex = int.Parse(intRaw);
				IVoice voice = CharacterManager.Instance.ActiveCharacter.Value.controller;
				voice.CurrentMouth = mouthIndex;
			})),
			["switchvoice"] = ((Action<string, string>)((character, intRaw) =>
			{
				int mouthIndex = int.Parse(intRaw);
				IVoice voice = CharacterManager.Instance.GetCharacter(character).controller;
				voice.CurrentMouth = mouthIndex;
			})),
			["stopvoices"] = ((Action<string>)((boolean) =>
			{
				bool setter = bool.Parse(boolean);
				BlockPreviousSpeakersOnNextLine = setter;
			})),

			// Because of H scenes and the technical stuff where you cannot have different
			// - character names per voice, this is a bit of a hacky solution to appear
			// - so.
			["internalchar"] = ((Action<string, string, string>)((characterName, intRawVoice, newName) =>
			{
				int mouthIndex = int.Parse(intRawVoice);
				Character character = CharacterManager.Instance.GetCharacter(characterName);
				IVoice voice = character.controller;
				voice.CurrentMouth = mouthIndex;
				character.ChangeCharacterName(newName);
			})),
			["internalchar"] = ((Action<string, string>)((intRawVoice, newName) =>
			{
				int mouthIndex = int.Parse(intRawVoice);
				Character character = CharacterManager.Instance.ActiveCharacter.Value;
				IVoice voice = character.controller;
				voice.CurrentMouth = mouthIndex;
				character.ChangeCharacterName(newName);
			})),
		};
		public static AudioClip GetVoiceLine(int index, ScriptHandler handler)
		{
			Document current = new(handler.document.ReadFile);
			string currentVisualPath = GetVisualPath(current);
			string filePath = $"Voice\\{currentVisualPath}\\{index}";
			AudioClip clip = Resources.Load<AudioClip>(filePath);
			if (clip == null)
			{
				// try again but with default
				string coreFilePath = $"Voice\\{GetVisualPath(current.GetWithoutLanguage())}\\{index}";
				Debug.Log($"'{filePath}' doesn't appear to work to get the normal voice line, using '{coreFilePath}' instead.");
				clip = Resources.Load<AudioClip>(coreFilePath);
				if (clip == null)
					Debug.LogWarning(new IOException($"Voiceline in resources path '{filePath}' " +
						"could not be retrieved.").ToString());
			}
			return clip;

			string GetVisualPath(Document doc) => doc.VisualPath.Remove(current.VisualPath.LastIndexOf('.'));
		}
		public static VoiceActorHandler GetNewActor(AudioSource source)
		{
			var output = source.gameObject.AddComponent<VoiceActorHandler>();
			output.AudioSource = source;
			return output;
		}
		public static bool BlockPreviousSpeakersOnNextLine { get; set; } = true;

		public AudioSource AudioSource { get; set; }
		private AudioClip currentVoiceLine;
		public bool IsPlaying
		{
			get => AudioSource != null && AudioSource.isPlaying;
			set 
			{
				if (AudioSource != null && AudioSource.isPlaying != value)
					if (value)
						AudioSource.Play();
					else
						AudioSource.Stop();
			}
		}

		public string ClipName => AudioSource != null ? AudioSource.clip.name : string.Empty;
		public float Volume 
		{ 
			get => AudioSource != null ? AudioSource.volume : float.NaN;
			set { if (AudioSource != null) AudioSource.volume = value; }
		}
		float IAudioInfo.Pitch 
		{ 
			get => AudioSource != null ? AudioSource.pitch : float.NaN;
			set { if (AudioSource != null) AudioSource.pitch = value; } 
		}
		bool IAudioInfo.Loop
		{
			get => AudioSource != null ? AudioSource.loop : false;
			set { if (AudioSource != null) AudioSource.loop = value; }
		}
		public AudioMixerGroup CurrentGroup
		{
			get => AudioSource.outputAudioMixerGroup;
			set => AudioSource.outputAudioMixerGroup = value;
		}
		public TimeSpan PlayedSeconds => AudioSource != null ? TimeSpan.FromSeconds(AudioSource.time) : TimeSpan.Zero;
		public TimeSpan TotalSeconds => AudioSource != null ? TimeSpan.FromSeconds(AudioSource.clip.length) : TimeSpan.Zero;

		private void OnEnable()
		{
			if (AudioSource != null)
				return;
			AudioSource = GetSource();

			AudioSource GetSource()
			{
				if (gameObject.TryGetComponent(out AudioSource audioSource))
					return audioSource;
				AudioSource output;
				if (gameObject.TryGetComponent(out CubismAudioMouthInput cubismAudioMouthInput) && cubismAudioMouthInput.AudioInput != null)
					return cubismAudioMouthInput.AudioInput;
				output = gameObject.AddComponent<AudioSource>();
				if (cubismAudioMouthInput != null)
					cubismAudioMouthInput.AudioInput = output;
				return output;
			}
		}
		public void Stop()
		{
			if (AudioSource != null)
				AudioSource.Stop();
		}
		public void Play(ScriptLine line)
		{
			Play(GetVoiceLine(line.Index, ScriptHandler.Instance));
		}
		public void Play(AudioClip clip)
		{
			currentVoiceLine = clip;
			if (BlockPreviousSpeakersOnNextLine && CharacterManager.HasInstance)
				for (int i = 0; i < CharacterManager.Instance.CharactersInScene.Count; i++)
					using (var enumerator2 = CharacterManager.Instance.CharactersInScene[i].controller.Mouths.GetEnumerator())
						while (enumerator2.MoveNext())
							enumerator2.Current.Value.Stop();
			AudioSource.loop = false;
			AudioSource.clip = currentVoiceLine;
			AudioSource.Play();
		}
	}
}