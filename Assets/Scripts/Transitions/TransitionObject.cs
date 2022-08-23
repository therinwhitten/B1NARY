namespace B1NARY.UI
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;
	//using TransitionData = System.Tuple<UnityEngine.GameObject, UnityEngine.Animator>;

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

		private TransitionStatus transitionStatus = TransitionStatus.Opaque;
		public TransitionStatus TransitionStatus => transitionStatus;

		public float AnimatorSpeed { get => animator.speed; set => animator.speed = value; }
		private Animator animator;

		public event Action FinishedTransition;
		private void Start()
		{
			animator = GetComponent<Animator>();
		}
		public async Task SetToOpaque(float speedMultiplier = 1f)
		{
			animator.SetTrigger(OpaqueTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			while (transitionStatus != TransitionStatus.Opaque)
				await Task.Yield();
			FinishedTransition?.Invoke();
		}
		public async Task SetToTransparent(float speedMultiplier = 1f)
		{
			animator.SetTrigger(TransparentTriggerName);
			animator.speed = speedMultiplier;
			transitionStatus = TransitionStatus.Running;
			while (transitionStatus != TransitionStatus.Opaque)
				await Task.Yield();
			FinishedTransition?.Invoke();
		}

		public void SetAnimationStatus(TransitionStatus transitionStatus)
		{
			this.transitionStatus = transitionStatus;
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