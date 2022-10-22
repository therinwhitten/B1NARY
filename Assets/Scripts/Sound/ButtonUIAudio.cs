namespace B1NARY.Audio 
{
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

   //Script for allowing custom sounds to be added to mouse movements. 

  public class ButtonUIAudio : MonoBehaviour

  {
      //Section to add different options for audio files in the Component

    public AudioSource UISounds;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip releaseSound;
   

      //Section to allow you to choose a one shot, which allows custom audio files. 

    public void Hoversound() 
    {
        UISounds.PlayOneShot (hoverSound);
    }
    public void ClickSound() 
     {
        UISounds.PlayOneShot (clickSound);
    }
    public void ReleaseSound() 
     {
        UISounds.PlayOneShot (releaseSound);
    }
  
  }

}
