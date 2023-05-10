using B1NARY;
using B1NARY.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BrightLib.Animation.Runtime
{
	public class PlayAudioClip : StateMachineBehaviour
	{
		public bool useMultiple;
		public AudioClip clip;
		public AudioClip[] clips;
		public AudioMixerGroup group;

		public PlayCondition condition;
		public Delayer delayer;
		public Timer frequencyTimer;

		public AudioSource _source;

		private CoroutineWrapper audioStuff;
		

		private int _clipIndex;
		private bool _valid;

		private int lastTick;
		private List<AudioTracker> trackers = new List<AudioTracker>();

		
		private void OnEnable()
		{
			delayer.OnComplete += Execute;
			frequencyTimer.onComplete += Execute;
		}

		private void OnDisable()
		{
			delayer.OnComplete -= Execute;
			frequencyTimer.onComplete -= Execute;
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			Validate(animator, stateInfo);

			delayer.Reset();
			frequencyTimer.Reset();
			frequencyTimer.loops = true;
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (condition == PlayCondition.OnEnter)
			{
				delayer.Update();
			}
			else if (condition == PlayCondition.OnUpdate)
			{
				frequencyTimer.Update();
			}			
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (condition != PlayCondition.OnExit) return;
			Execute();
		}

		private void Execute()
		{
			if (!_valid) 
				return;
			if (lastTick == Time.frameCount)
			{
				AudioSource subSource = _source.gameObject.AddComponent<AudioSource>();
				subSource.outputAudioMixerGroup = group;
				AudioTracker tracker = new AudioTracker(null, subSource).Start();
				tracker.Loop = true;
				trackers.Add(tracker);
				audioStuff.AfterActions += (mono) =>
				{
					tracker.Stop();
					Destroy(subSource);
					trackers.Remove(tracker);
				};
			}
			lastTick = Time.frameCount;
			_source.Stop();
			_source.outputAudioMixerGroup = group;
			_source.loop = true;
			if (!CoroutineWrapper.IsNotRunningOrNull(audioStuff))
				audioStuff.Stop();
			audioStuff = new CoroutineWrapper(SceneManager.Instance, ExecuteLoop()).Start();
		}

		private void Validate(Animator animator, AnimatorStateInfo stateInfo)
		{
			_valid = false;
			if (_source == null)
			{
				_source = animator.GetComponent<AudioSource>();
				if (_source == null)
				{
					Debug.LogWarning($"{animator.name}.{nameof(PlayAudioClip)}: No {nameof(AudioSource)} found on {animator.name}.");
					return;
				}
			}

			if (!useMultiple)
			{
				if (clip == null)
				{
					Debug.LogWarning($"{animator.name}.{nameof(PlayAudioClip)}: No {nameof(AudioClip)} added.");
					return;
				}
			}
			else
			{
				if (useMultiple && clips == null || clips.Length == 0)
				{
					Debug.LogWarning($"{animator.name}.{nameof(PlayAudioClip)}: No {nameof(AudioClip)}s added.");
					return;
				}	
			}
			_valid = true;
		}

		private IEnumerator ExecuteLoop()
		{
			while (true)
			{
				for (int i = 0; i < trackers.Count; i++)
				{
					AudioTracker track = trackers[i];
					if (track.IsPlaying)
						continue;
					AudioClip newClip = useMultiple ? clips.Random(RandomFowarder.RandomType.Doom) : clip;
					track.Dispose();
					trackers[i] = new AudioTracker((CustomAudioClip)newClip, track.audioSource).Start();
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}

}