namespace B1NARY
{
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public sealed class TabButton : MonoBehaviour
	{
		public Button Button { get; private set; }
		public GameObject contents;
		private void Awake()
		{
			Button = GetComponent<Button>();
		}
	}
}