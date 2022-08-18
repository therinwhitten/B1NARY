namespace B1NARY
{
	using System;
	using System.Threading.Tasks;
	using UnityEngine;

	[RequireComponent(typeof(Animator))]
	public class TransitionSlave : MonoBehaviour
	{
		public bool TransitionActive
		{
			get => animator.GetBool(boolName);
			set => animator.SetBool(boolName, value);
		}
		public enum TransitionStatus : byte
		{
			FadedIn,
			FadedOut,
		}

		[SerializeField] private string boolName = "Transition";
		private Animator animator;
		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		public Task StartAnimation()
		{
			TransitionActive = true;
			while (true)
			{
				if (finishedTask.HasValue)
					if (finishedTask.Value == TransitionStatus.FadedIn)
						break;
				Task.Yield();
			}
			finishedTask = null;
			return Task.CompletedTask;
		}
		public Task StopAnimation()
		{
			TransitionActive = false;
			while (true)
			{
				if (finishedTask.HasValue)
					if (finishedTask.Value == TransitionStatus.FadedOut)
						break;
				Task.Yield();
			}
			finishedTask = null;
			return Task.CompletedTask;
		}

		private TransitionStatus? finishedTask = null;

		public void Finished(TransitionStatus transitionStatus)
		{
			finishedTask = transitionStatus;
		}
	}
}
