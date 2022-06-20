using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
	[Header("Audio Sounds"), SerializeField] private AudioClip hover; 
	[SerializeField] private AudioClip press;
	private AudioHandler audioMaster;

	[Header("Other"), SerializeField] Animator animator;

	[SerializeField]
	GameObject controller;
	CanvasGroup canvasGroup;
	bool Interactible => canvasGroup.interactable;
	bool resized = false;
	BoxCollider2D col;
	RectTransform rect;

	[SerializeField]
	string action;

	private void Start()
	{
		col = gameObject.GetComponent<BoxCollider2D>();
		rect = gameObject.GetComponent<RectTransform>();
		canvasGroup = controller.GetComponent<CanvasGroup>();

		// It's a better idea to have these menu buttons get the audioMaster by
		// - its parent instead of every button getting it individually.
		audioMaster = FindObjectOfType<AudioHandler>();
	}

	private void Update()
	{
		if (col.size != rect.sizeDelta)
		{
			col.size = rect.sizeDelta;
		}
		col.enabled = Interactible;
	}

	private void OnMouseEnter()
	{
		if (!Interactible)
			return;
		audioMaster.PlayOneShot(hover);
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
		audioMaster.PlaySound(press);
		animator.SetBool("pressed", true);
		controller.SendMessage(action);
	}
	private void OnMouseUp()
	{
		animator.SetBool("pressed", false);
	}
}
