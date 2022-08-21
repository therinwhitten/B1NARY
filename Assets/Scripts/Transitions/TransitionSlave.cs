namespace B1NARY
{
	using System;
	using System.Threading.Tasks;
	using UnityEngine;

	[RequireComponent(typeof(Animator)), Obsolete]
	public class TransitionSlave : MonoBehaviour
	{
		public enum TransitionStatus : byte
		{
			FadedIn,
			FadedOut,
		}

		private TaskCompletionSource<TransitionStatus> completionSource = null;
		private Animator animator;
		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		[SerializeField] private string boolParameterName = "Transition";
		public async Task StartAnimation()
		{
			animator.SetBool(boolParameterName, true);
			retry:
			completionSource = new TaskCompletionSource<TransitionStatus>();
			await completionSource.Task;
			if (completionSource.Task.Result != TransitionStatus.FadedIn)
				goto retry;
		}
		public async Task StopAnimation()
		{
			animator.SetBool(boolParameterName, false);
			retry:
			completionSource = new TaskCompletionSource<TransitionStatus>();
			await completionSource.Task;
			if (completionSource.Task.Result != TransitionStatus.FadedOut)
				goto retry;
		}



		/// <summary>
		/// Declares similar to an event to the <see cref="TransitionSlave"/>
		/// that the animation has reached the end of a stage. Depending on what
		/// method is active, it will simply continue if it doesn't match the key.
		/// </summary>
		/// <remarks>
		/// Allows you to use the same animation but reversed if needed.
		/// </remarks>
		public void FinishStage(TransitionStatus transitionStatus)
		{
			completionSource.SetResult(transitionStatus);
		}
	}
}
