namespace B1NARY
{
	using UnityEngine;
	using UnityEngine.UI;

	/// <summary>
	/// This script takes all of the transparency out of an image being used as a 
	/// button to enable only the image to be the hitbox
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class CreativeButtons : MonoBehaviour
	{
		void Awake()
		{
			GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
		}
	}
}
