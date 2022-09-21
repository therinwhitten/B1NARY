namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.Events;

	public class Switch : MonoBehaviour
	{
		public UnityEvent trueEvents, falseEvents;
		public void InvokeEvents(bool value)
		{
			if (value)
				trueEvents.Invoke();
			else
				falseEvents.Invoke();
		}
		public void InvokeEvents(GameObject gameObject) =>
			InvokeEvents(gameObject.activeSelf);
	}
}