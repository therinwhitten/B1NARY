namespace B1NARY
{
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public sealed class TabButton : MonoBehaviour
	{
		private Button m_button;
		public Button Button 
		{
			get 
			{
				if (m_button == null)
					m_button = GetComponent<Button>();
				return m_button;
			} 
		}
		public GameObject contents;
	}
}