using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip select, press;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        audio.Stop();
        audio.PlayOneShot(select);
        animator.SetBool("selected", true);
        Debug.Log("Mouse Enter");
    }
    private void OnMouseExit()
    {
        animator.SetBool("selected", false);
        Debug.Log("Mouse Exit");
    }
    private void OnMouseDown()
    {
        audio.Stop();
        audio.PlayOneShot(press);
        animator.SetBool("pressed", true);
        Debug.Log("Clicked");
    }
    private void OnMouseUp()
    {
        animator.SetBool("pressed", false);
    }
}
