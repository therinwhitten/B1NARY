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
		public async Task SetToOpaque(float speedMultiplier = 1f)
		{
			if (!enabled)
				return;
			animator.SetTrigger(OpaqueTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			await Task.Delay(50); // Compatibility for reversed animations
			await YieldUntil();
		}
		public IEnumerator SetToOpaqueEnumerator(float speedMultiplier = 1f)
		{
			if (!enabled)
				yield break;
			animator.SetTrigger(OpaqueTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			while (transitionStatus != TransitionStatus.Opaque)
				yield return new WaitForEndOfFrame();
		}
		public async Task SetToTransparent(float speedMultiplier = 1f)
		{
			if (!enabled)
				return;
			animator.SetTrigger(TransparentTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			await Task.Delay(50); // Compatibility for reversed animations
			await YieldUntil();
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
		private async Task YieldUntil()
		{
			while (TransitionStatus == TransitionStatus.Running)
				await Task.Yield();
			//retry:
			//var completionSource = new TaskCompletionSource<TransitionStatus>();
			//ChangedTransitionStatus += completionSource.SetResult;
			//await completionSource.Task;
			//if (completionSource.Task.Result != transitionStatus)
			//	goto retry;
			//ChangedTransitionStatus -= completionSource.SetResult;
		}

		public void SetAnimationStatus(TransitionStatus transitionStatus)
		{
			TransitionStatus = transitionStatus;
		}



		/*
		[SerializeField]
		private string opaqueTriggerName = "";

		private TransitionData[] transitionOverlays;
		private int currentTransitionIndex = 0;
		private void Start()
		{
			transitionOverlays = GetTransitions().ToArray();
		}
		private List<TransitionData> GetTransitions()
		{
			Transform currentTransform = gameObject.transform;
			if (currentTransform.childCount == 0)
				throw new InvalidOperationException($"'{name}' doesn't have any"
					+ "children to work with! Put in at least 1 transition!");
			var children = new List<TransitionData>(currentTransform.childCount);
			var exceptions = new List<Exception>();
			for (int i = 0; i < children.Capacity; i++)
				try
				{
					Transform child = currentTransform.GetChild(i);
					if (!child.gameObject.TryGetComponent<Canvas>(out _))
						throw new MissingComponentException($"{child.name} although exists, doesn't have a {nameof(Canvas)}!");
					if (child.gameObject.TryGetComponent<Animator>(out var animator))
						// expected end result here.
						children.Add(new TransitionData(child.gameObject, animator));
					else
						throw new MissingComponentException($"{child.name} does not have {nameof(Animator)}!");
				}
				catch (Exception ex) { exceptions.Add(ex); }
			if (exceptions.Any())
				Debug.LogError(new AggregateException("Some children doesn't follow the requirements: ", exceptions));
			return children;
		}
		*/
	}
}