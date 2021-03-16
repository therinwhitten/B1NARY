using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChoiceButton : MonoBehaviour
{

    [SerializeField] Animator animator;
    // public int x = 0, y = 0;
    GameObject controller;
    public Text text;
    string choiceName;
    CanvasGroup canvasGroup;
    bool resized = false;
    BoxCollider2D col;
    RectTransform rect;
    bool interactable { get { return canvasGroup.interactable; } }
    public DialogueNode node;
    // Start is called before the first frame update
    void Start()
    {
        col = gameObject.GetComponent<BoxCollider2D>();
        rect = gameObject.GetComponent<RectTransform>();
        controller = GameObject.Find("Choice Panel");
        canvasGroup = controller.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        // i really hate that I have to do this but I can't find
        // a better way to resize the collider after I generate the text
        // at least I'm not comparing the two values on every frame

        if (!resized)
        {
            if (col.size != rect.sizeDelta)
            {
                col.size = rect.sizeDelta;
            }
            resized = true;
        }
        col.enabled = interactable;
    }
    public void assignName(string name)
    {
        rect = gameObject.GetComponent<RectTransform>();
        choiceName = name;
        text.text = name;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
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
            ScriptParser.Instance.currentNode.selectChoice(node);
            AudioManager.Instance.Play("Button-Press", true);
            animator.SetBool("pressed", true);
            controller.SendMessage("fadeOut");
            // controller.SendMessage(action);
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
