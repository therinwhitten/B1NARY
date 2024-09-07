namespace B1NARY.UI
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;

	public enum TransitionStatus
	{
		[Tooltip("The transition is transferring to another stage.")]
		Running = 16,
		[Tooltip("Whenever the user cannot see the screen at all due to the transition and is at a stationary state that determines which it will not change by itself.")]
		Opaque = 0,
		[Tooltip("Whenever the user sees the screen clearly without any interference from transitions")]
		Transparent = 8,
	}

	// Starts at end, but starts at beginning
	[RequireComponent(typeof(Canvas), typeof(Animator))]
	public class TransitionObject : Multiton<TransitionObject>
	{
		#region Output From the User
		[SerializeField]
		private string opaqueTriggerName = "Opaque",
			transparentTriggerName = "Transparent";
		public string OpaqueTriggerName => opaqueTriggerName;
		public string TransparentTriggerName => transparentTriggerName;
		#endregion

		public event Action<TransitionStatus> ChangedTransitionStatus;
		private TransitionStatus transitionStatus = TransitionStatus.Opaque;
		public TransitionStatus TransitionStatus
		{
			get => transitionStatus;
			set
			{
				transitionStatus = value;
				ChangedTransitionStatus?.Invoke(value);
			}
		}

		public float AnimatorSpeed { get => animator.speed; set => animator.speed = value; }
		private Animator animator;

		// Unity doesn't detect if the actual 'is enabled' values are being used,
		// - and this disallows the usage of turning it off. Creating these methods
		// - allows to subvert that.
		private void OnEnable() { }
		private void OnDisable() { }

		protected override void MultitonAwake()
		{
			animator = GetComponent<Animator>();
			Canvas canvas = GetComponent<Canvas>();
			if (canvas.worldCamera == null)
				canvas.worldCamera = FindObjectOfType<Camera>();
		}
		public IEnumerator SetToOpaqueEnumerator(float speedMultiplier = 1f)
		{
			if (!enabled)
				yield break;
			animator.speed = speedMultiplier;
			animator.SetTrigger(OpaqueTriggerName);
			transitionStatus = TransitionStatus.Running;
			while (transitionStatus != TransitionStatus.Opaque)
				yield return new WaitForEndOfFrame();
		}
		public IEnumerator SetToTransparentEnumerator(float speedMultiplier = 1f)
		{
			if (!enabled)
				yield break;
			animator.SetTrigger(OpaqueTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			while (transitionStatus != TransitionStatus.Transparent)
				yield return new WaitForEndOfFrame();
		}

		public void SetAnimationStatus(TransitionStatus transitionStatus)
		{
			TransitionStatus = transitionStatus;
		}
	}
}