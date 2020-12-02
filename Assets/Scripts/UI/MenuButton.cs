using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("selected", true);
        Debug.Log("Mouse enter");
    }

    private void OnMouseEnter()
    {
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
        Debug.Log("Clicked");
    }
}
