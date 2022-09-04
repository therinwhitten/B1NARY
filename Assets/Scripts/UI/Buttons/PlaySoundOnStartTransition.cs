namespace B1NARY.UI
{
	using UnityEngine;

	public class PlaySoundOnStartTransition : StateMachineBehaviour
	{
		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public int soundIndex;
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (animator.TryGetComponent<ButtonSoundBehaviour>(out var buttonSound))
				buttonSound.PlaySound(soundIndex);
			else
				Debug.LogError("Cannot find a valid output to play sounds!", this);
		}
	}
}