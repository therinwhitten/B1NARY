using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    [SerializeField] Animator animator;
    // public int x = 0, y = 0;
    // [SerializeField] AudioSource audio;
    // [SerializeField] AudioClip selectSound, pressSound;

    [SerializeField]
    GameObject controller;
    CanvasGroup canvasGroup;
    bool interactable { get { return canvasGroup.interactable; } }
    bool resized = false;
    BoxCollider2D col;
    RectTransform rect;

    [SerializeField]
    string action;
    // Start is called before the first frame update
    void Start()
    {
        col = gameObject.GetComponent<BoxCollider2D>();
        rect = gameObject.GetComponent<RectTransform>();
        canvasGroup = controller.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (col.size != rect.sizeDelta)
        {
            col.size = rect.sizeDelta;
        }
    }

    private void OnMouseEnter()
    {
        if (interactable)
            select();
    }
    private void OnMouseExit()
    {
        animator.SetBool("selected", false);
    }
    private void OnMouseDown()
    {
        if (interactable)
        {
            // audio.Stop();
            // audio.PlayOneShot(pressSound);
            AudioManager.Instance.Play("Button-Press", true);
            animator.SetBool("pressed", true);
            controller.SendMessage(action);
        }
    }
    private void OnMouseUp()
    {
        animator.SetBool("pressed", false);
    }
    public void select()
    {
        // audio.Stop();
        // audio.PlayOneShot(selectSound);
        AudioManager.Instance.Play("Button-Select", true);
        animator.SetBool("selected", true);
    }
}
