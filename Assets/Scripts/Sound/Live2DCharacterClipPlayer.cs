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
		private Queue<Live2DCharacterClipPlayer> playerStates = new Queue<Live2DCharacterClipPlayer>();

		public AudioClip[] playableClips;
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
				while (playerStates.Count > 0)
					playerStates.Dequeue().targetMouth.AudioInput.Stop();
			}
			playerStates.Enqueue(this);
			targetMouth.AudioInput.Stop();
			targetMouth.AudioInput.clip = playableClips.Random();
			targetMouth.AudioInput.Play();
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
			clipPlayer.randomType = DirtyAuto.Popup(clipPlayer, new GUIContent("Random Type", "Niche controlling option. Allows you to assign which randomization tool you want to apply when using multiple clips"), clipPlayer.randomType);
			clipPlayer.TargetSpeaker = DirtyAuto.Field(clipPlayer, new GUIContent("Target Speaker (Multiple Mouths)", "An index or tag system to allow multiple mouths to play for a single character. If you have a single mouth, leave this blank."), clipPlayer.TargetSpeaker);
		}
	}
}
#endif