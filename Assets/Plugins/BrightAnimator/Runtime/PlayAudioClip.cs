using B1NARY;
using B1NARY.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BrightLib.Animation.Runtime
{
	public class PlayAudioClip : StateMachineBehaviour
	{
		private static int globalFrame = -1;

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

		private List<AudioSource> trackers;
		private (Animator animator, int layer) lastUsedAnimator;

		
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
			lastUsedAnimator = (animator, layerIndex);

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
			lastUsedAnimator = (animator, layerIndex);
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (condition != PlayCondition.OnExit) return;
			Execute();
			lastUsedAnimator = (animator, layerIndex);
		}

		private void Execute()
		{
			if (!_valid) 
				return;
			if (Math.Abs(globalFrame -Time.frameCount) > 5)
			{
				_source.Stop();
				_source.outputAudioMixerGroup = group;
				_source.loop = true;
				if (!CoroutineWrapper.IsNotRunningOrNull(audioStuff))
					audioStuff.Stop();
				if (trackers is null)
					trackers = new List<AudioSource>() { _source };
				audioStuff = new CoroutineWrapper(SceneManager.Instance, ExecuteLoop()).Start();
				return;
			}
			globalFrame = Time.frameCount;
			AudioSource subSource = _source.gameObject.AddComponent<AudioSource>();
			subSource.outputAudioMixerGroup = group;
			subSource.loop = true;
			trackers.Add(subSource);
			audioStuff.AfterActions += (mono) =>
			{
				Destroy(subSource);
				trackers.Remove(subSource);
			};
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
					AudioSource source = trackers[i];
					if (source.isPlaying)
						continue;
					AudioClip newClip = useMultiple ? clips.Random(RandomForwarder.RandomType.CSharp) : clip;
					source.Stop();
					source.clip = newClip;
					source.Play();
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}

}