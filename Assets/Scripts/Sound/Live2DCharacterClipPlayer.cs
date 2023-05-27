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
		private CubismAudioMouthInput targetMouth;

		[Space]
		public int TargetSpeaker = 0;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (targetMouth == null)
			{
				CubismAudioMouthInput[] mouths = animator.gameObject.GetComponents<CubismAudioMouthInput>();
				for (int i = 0; i < mouths.Length; i++)
				{
					CubismAudioMouthInput mouth = mouths[i];
					if (mouth.TargetMouth != TargetSpeaker)
						continue;

					if (targetMouth != null)
						throw new ArgumentException($"There is more than 1 mouths that target the tag: '{TargetSpeaker}'!", nameof(TargetSpeaker));
					targetMouth = mouth;
				}
				if (targetMouth == null)
					throw new ArgumentException($"'{TargetSpeaker}' is not linked to a proper {nameof(CubismAudioMouthInput)}!", nameof(TargetSpeaker));
			}

			if (lastFrame != Time.frameCount)
			{
				Debug.Log("Disposing");
				lastFrame = Time.frameCount;
				for (int i = 0; i < playerStates.Count; i++)
					playerStates[i].player.targetMouth.AudioInput.Stop();
				playerStates.Clear();
				trueLoop?.Stop();
			}
			IEnumerator<bool> clipPlayer = PlayNewRandomClip();
			clipPlayer.MoveNext(); // start play
			playerStates.Add((clipPlayer, this));
			// This will cause all sounds to act if it is truly randomized if enabled,
			// - but i dont think anyone sensible enough will actually do that.
			if (trueRandom && CoroutineWrapper.IsNotRunningOrNull(trueLoop))
				trueLoop = new CoroutineWrapper(targetMouth, TrueRandom()).Start();
			targetMouth.AudioInput.loop = loop;
		}

		public IEnumerator<bool> PlayNewRandomClip()
		{
			targetMouth.AudioInput.Stop();
			AudioClip clip = playableClips.Random();
			targetMouth.AudioInput.clip = clip;
			targetMouth.AudioInput.Play();
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