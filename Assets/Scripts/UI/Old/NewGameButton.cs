using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGameButton : MonoBehaviour
{
	public CanvasGroup menuCanvas;
	public CanvasGroup dialogueCanvas;
	private bool fadingOut = false;
	private bool fadeIn = false;
	public float fadeSpeed = 0.1f;
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (fadingOut && menuCanvas.alpha > 0)
		{
			menuCanvas.alpha = Mathf.MoveTowards(menuCanvas.alpha, 0, fadeSpeed);
		}
		if (menuCanvas.alpha == 0)
		{
			fadeIn = true;
		}
		if (fadeIn && dialogueCanvas.alpha < 1)
		{
			dialogueCanvas.alpha = Mathf.MoveTowards(dialogueCanvas.alpha, 1, fadeSpeed);
		}
		if (dialogueCanvas.alpha == 1)
		{
			DialogueSystem.Instance.initialize();
			ScriptParser.Instance.initialize();
			menuCanvas.gameObject.SetActive(false);
		}

	}
	private void OnMouseDown()
	{
		fadingOut = true;
	}
}
