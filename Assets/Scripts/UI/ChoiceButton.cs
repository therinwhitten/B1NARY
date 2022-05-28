using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChoiceButton : MonoBehaviour
{
	[SerializeField] AudioClip hover, press;
	private AudioHandler audioMaster;

	[SerializeField] Animator animator;
	// public int x = 0, y = 0;
	GameObject controller;
	public Text text;
	string choiceName;
	CanvasGroup canvasGroup;
	bool resized = false;
	BoxCollider2D col;
	RectTransform rect;
	bool Interactible => canvasGroup.interactable; 
	public DialogueNode node;
	// Start is called before the first frame update
	void Start()
	{
		col = gameObject.GetComponent<BoxCollider2D>();
		rect = gameObject.GetComponent<RectTransform>();
		controller = GameObject.Find("Choice Panel");
		canvasGroup = controller.GetComponent<CanvasGroup>();
		audioMaster = FindObjectOfType<AudioHandler>();
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
		col.enabled = Interactible;
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
		if (!Interactible)
			return;
		audioMaster.PlaySound(hover);
		animator.SetBool("selected", true);
	}
	private void OnMouseExit()
	{
		animator.SetBool("selected", false);
	}
	private void OnMouseDown()
	{
		if (!Interactible)
			return;
		ScriptParser.Instance.currentNode.selectChoice(node);
		audioMaster.PlaySound(press);
		animator.SetBool("pressed", true);
		controller.SendMessage("fadeOut");
	}
	private void OnMouseUp()
	{
		animator.SetBool("pressed", false);
	}
}
