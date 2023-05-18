namespace B1NARY.Audio
{
	using MEC;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.Audio;

	public class AudioClipPlayer : StateMachineBehaviour
	{
		public static List<AudioState> states = new List<AudioState>();
		private static AudioSource motherOfAllSources = null;
		private static CoroutineHandle handle;
		private static int lastFrame = -1;

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
					throw new Exception("balls");

			if (lastFrame != Time.frameCount)
			{
				Debug.Log("Disposing");
				lastFrame = Time.frameCount;
				Timing.KillCoroutines(handle);
				motherOfAllSources.Stop();
				states.Clear();

				handle = Timing.RunCoroutine(UpdateCoroutine());
			}
			switch (statePlayType)
			{
				case StatePlayType.All:
					for (int i = 0; i < playableClips.Length; i++)
						states.Add(new AudioState(motherOfAllSources, playableClips[i]));
					break;
				case StatePlayType.Random:
					states.Add(new AudioState(motherOfAllSources, playableClips.Random(randomType)));
					break;
			}
		}

		IEnumerator<float> UpdateCoroutine()
		{
			while (true)
			{
				for (int i = 0; i < states.Count; i++)
					states[i] = states[i].Update();
				yield return Timing.WaitForOneFrame;
			}
		}

		public struct AudioState : IDisposable
		{
			public AudioClip clip;
			public AudioSource target;
			public TimeSpan ticksLeft;
			public AudioState(AudioSource target, AudioClip clip)
			{
				this.clip = clip;
				this.target = target;
				ticksLeft = TimeSpan.Zero;
			}

			public AudioState Update()
			{
				ticksLeft -= TimeSpan.FromSeconds(Time.deltaTime);
				if (ticksLeft < TimeSpan.Zero)
				{
					ticksLeft = TimeSpan.FromSeconds(clip.length);
					target.PlayOneShot(clip);
				}
				return this;
			}
			public void Dispose()
			{
				target.Stop();
			}
		}
	}

	public enum StatePlayType
	{
		Random,
		All
	}
}
