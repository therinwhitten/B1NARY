namespace B1NARY.Audio
{
	using System;
	using UnityEngine;

	[Obsolete("Use 'B1NARY.UI.ButtonSoundBehaviour' for more customizability!")]
	public class ButtonUIAudio : MonoBehaviour
	{
		public AudioSource UISounds;
		public AudioClip hoverSound;
		public AudioClip clickSound;
	
		public void Hoversound()
		{
			UISounds.PlayOneShot (hoverSound);
		}
		public void ClickSound()
		{
			UISounds.PlayOneShot (clickSound);
		}
	}
}
