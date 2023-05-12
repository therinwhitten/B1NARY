namespace B1NARY.Audio
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.Audio;

	public class AudioClipPlayer : StateMachineBehaviour
	{
		private static readonly List<AudioSource> extraAudioSources = new List<AudioSource>();
		private static AudioSource motherOfAllSources = null;
		private static int stateName = -1;

		public AudioClip[] playableClips;
		public StatePlayType statePlayType;
		public RandomForwarder.RandomType randomType = RandomForwarder.RandomType.CSharp;

		[Space]
		public AudioMixerGroup group;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (motherOfAllSources == null)
				if (animator.gameObject.TryGetComponent<AudioSource>(out var source))
					motherOfAllSources = source;
				else
					motherOfAllSources = animator.gameObject.AddComponent<AudioSource>();

			if (stateName != stateInfo.shortNameHash)
			{
				stateName = stateInfo.shortNameHash;
				for (int i = 0; i < extraAudioSources.Count; i++)
				{
					extraAudioSources[i].Stop();
					Destroy(extraAudioSources[i]);
				}
				extraAudioSources.Clear();
				motherOfAllSources.Stop();
				
				switch (statePlayType)
				{
					case StatePlayType.All:
						for (int i = 0; i < playableClips.Length; i++)
							AddNewAudioSource(playableClips[i]);
						break;
					case StatePlayType.Random:
						AddNewAudioSource(playableClips.Random(randomType));
						break;
				}
			}
		}
		
		private void AddNewAudioSource(AudioClip clip)
		{
			if (!motherOfAllSources.isPlaying)
			{
				motherOfAllSources.clip = clip;
				motherOfAllSources.Play();
			}
			var source = motherOfAllSources.gameObject.AddComponent<AudioSource>();
			extraAudioSources.Add(source);
			Array.ForEach(typeof(AudioSource).GetFields(BindingFlags.NonPublic | BindingFlags.Instance),
				(field) => field.SetValue(source, field.GetValue(motherOfAllSources)));
			source.outputAudioMixerGroup = group;
		}
	}

	public enum StatePlayType
	{
		Random,
		All
	}
}
