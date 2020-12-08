using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    [SerializeField] Animator animator;
    public int x = 0, y = 0;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip selectSound, pressSound;
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
        select();
    }
    private void OnMouseExit()
    {
        animator.SetBool("selected", false);
        Debug.Log("Mouse Exit");
    }
    private void OnMouseDown()
    {
        audio.Stop();
        audio.PlayOneShot(pressSound);
        animator.SetBool("pressed", true);
        Debug.Log("Clicked");
    }
    private void OnMouseUp()
    {
        animator.SetBool("pressed", false);
    }
    public void select()
    {
        audio.Stop();
        audio.PlayOneShot(selectSound);
        animator.SetBool("selected", true);
    }
}
