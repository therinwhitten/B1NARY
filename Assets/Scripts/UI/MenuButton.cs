using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
	[Header("Audio Sounds"), SerializeField] private AudioClip hover; 
	[SerializeField] private AudioClip press;
	private AudioHandler AudioHandler => AudioHandler.Instance;

	[Header("Other"), SerializeField] Animator animator;

	[SerializeField]
	GameObject controller;
	CanvasGroup canvasGroup;
	bool Interactible => canvasGroup.interactable;
	bool resized = false;
	BoxCollider2D col;
	RectTransform rect;

	[SerializeField]
	private UnityEvent actions;

	private void Start()
	{
		col = gameObject.GetComponent<BoxCollider2D>();
		rect = gameObject.GetComponent<RectTransform>();
		canvasGroup = controller.GetComponent<CanvasGroup>();
		if (actions == null)
			actions = new UnityEvent();
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
		if (hover != null)
			AudioHandler.PlayOneShot(hover);
		else
			Debug.LogError($"Button {nameof(hover)} is not tied to an audioClip!");
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
		if (press != null)
			AudioHandler.PlayOneShot(press);
		else
			Debug.LogError($"Button {nameof(press)} is not tied to an audioClip!");
		animator.SetBool("pressed", true);
		Perform();
	}
	private void OnMouseUp()
	{
		animator.SetBool("pressed", false);
	}

	public virtual void Perform()
	{
		actions.Invoke();
		// For later when we actually get to input the input system.
		//Debug.Log($"Button '{gameObject.name}' doesn't have any assigned actions, " +
		//	$"try using inheriting {nameof(MenuButton)} and customizing the method" +
		//	$": {nameof(Perform)}");
	}
}
