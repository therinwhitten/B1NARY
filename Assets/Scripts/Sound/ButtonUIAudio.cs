namespace B1NARY.Audio
{
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

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
