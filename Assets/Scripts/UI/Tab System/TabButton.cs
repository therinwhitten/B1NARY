namespace B1NARY
{
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public sealed class TabButton : MonoBehaviour
	{
		private Button button;
		private void Awake()
		{
			button = GetComponent<Button>();
		}
	}
}