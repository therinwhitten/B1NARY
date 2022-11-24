namespace B1NARY.UI
{
	using System.Diagnostics;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// Beat you to it lol
	/// </summary>
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