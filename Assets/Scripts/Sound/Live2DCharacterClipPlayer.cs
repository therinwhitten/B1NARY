namespace B1NARY.Audio
{
	using Live2D.Cubism.Framework.MouthMovement;
	using MEC;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.Audio;

	public class Live2DCharacterClipPlayer : StateMachineBehaviour
	{
		private static int lastFrame = -1;
		private static CoroutineWrapper trueLoop;
		private static readonly List<(IEnumerator<bool> span, Live2DCharacterClipPlayer player)> playerStates = new List<(IEnumerator<bool> span, Live2DCharacterClipPlayer player)>();
		private static Queue<AudioSource> extraSources = new Queue<AudioSource>();
		private static IEnumerator TrueRandom()
		{
			while (true)
			{
				for (int i = 0; i < playerStates.Count; i++)
				{
					playerStates[i].span.MoveNext();
					bool giveNew = playerStates[i].span.Current;
					if (giveNew)
					{
						IEnumerator<bool> enumerator = playerStates[i].player.PlayNewRandomClip();
						enumerator.MoveNext();
						playerStates[i] = (enumerator, playerStates[i].player);
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}

		[Tooltip("Playable clips; clips are selected at random but only if there is more than 1 element.")]
		public AudioClip[] playableClips;
		public bool trueRandom;
		public bool loop = true;
		public RandomForwarder.RandomType randomType = RandomForwarder.RandomType.CSharp;
		private AudioSource targetMouth;
		public AudioMixerGroup group;

		[Space]
		public int TargetSpeaker = 0;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (targetMouth == null)
			{
				targetMouth = extraSources.Count > 0 ? extraSources.Dequeue() : animator.gameObject.AddComponent<AudioSource>();
				targetMouth.outputAudioMixerGroup = group;
			}

			if (lastFrame != Time.frameCount)
			{
				Debug.Log("Disposing");
				lastFrame = Time.frameCount;
				for (int i = 0; i < playerStates.Count; i++)
				{
					playerStates[i].player.targetMouth.Stop();
					extraSources.Enqueue(playerStates[i].player.targetMouth);
					playerStates[i].player.targetMouth = null;
				}
				playerStates.Clear();
				trueLoop?.Stop();
			}
			IEnumerator<bool> clipPlayer = PlayNewRandomClip();
			clipPlayer.MoveNext(); // start play
			playerStates.Add((clipPlayer, this));
			// This will cause all sounds to act if it is truly randomized if enabled,
			// - but i dont think anyone sensible enough will actually do that.
			if (trueRandom && CoroutineWrapper.IsNotRunningOrNull(trueLoop))
				trueLoop = new CoroutineWrapper(animator.GetComponent<MonoBehaviour>(), TrueRandom()).Start();
			targetMouth.loop = loop;
		}

		public IEnumerator<bool> PlayNewRandomClip()
		{
			targetMouth.Stop();
			AudioClip clip = playableClips.Random();
			targetMouth.clip = clip;
			targetMouth.Play();
			yield return false;

			// Time counter stuff here
			TimeSpan delay = TimeSpan.FromSeconds(clip.length);
			while (delay.Ticks > 0)
			{
				delay -= TimeSpan.FromSeconds(Time.deltaTime);
				yield return false;
			}
			yield return true;
		}

		
	}
}
#if UNITY_EDITOR
namespace B1NARY.Audio.Editor
{
	using B1NARY.Editor;
	using System;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(Live2DCharacterClipPlayer))]
	public class Live2CharacterStateClipEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			Live2DCharacterClipPlayer clipPlayer = (Live2DCharacterClipPlayer)target;
			DirtyAuto.Property(serializedObject, nameof(Live2DCharacterClipPlayer.playableClips));
			clipPlayer.loop = DirtyAuto.Toggle(clipPlayer, new GUIContent("Loop"), clipPlayer.loop);
			clipPlayer.trueRandom = DirtyAuto.Toggle(clipPlayer, new GUIContent("True Randomization", "Instead of picking a random option at the start and continue moving forward, this will instead will try to select a new random item every time it is finished"), clipPlayer.trueRandom);
			clipPlayer.randomType = DirtyAuto.Popup(clipPlayer, new GUIContent("Random Type", "Niche controlling option. Allows you to assign which randomization tool you want to apply when using multiple clips"), clipPlayer.randomType);
			clipPlayer.TargetSpeaker = DirtyAuto.Field(clipPlayer, new GUIContent("Target Speaker (Multiple Mouths)", "An index or tag system to allow multiple mouths to play for a single character. If you have a single mouth, leave this blank."), clipPlayer.TargetSpeaker);
		}
	}
}
#endif