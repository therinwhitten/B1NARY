namespace B1NARY.UI
{
	using System.Diagnostics;
	using UnityEngine;
	using UnityEngine.EventSystems;

	public sealed class HyperlinkInitializer : MonoBehaviour, IPointerClickHandler
	{
		public string hyperLink;
		void IPointerClickHandler.OnPointerClick(PointerEventData eventData) 
			=> PressHyperLink();

		public void PressHyperLink()
		{
			Process.Start(hyperLink);
		}
	}
}