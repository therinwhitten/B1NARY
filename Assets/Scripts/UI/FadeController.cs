using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{

    private CanvasGroup canvas;
    public bool fadingOut = false;
    public bool fadingIn = false;
    public float fadeSpeed = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        canvas = gameObject.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadingOut && canvas.alpha > 0)
        {
            canvas.alpha = Mathf.MoveTowards(canvas.alpha, 0, fadeSpeed);
        }
        if (canvas.alpha == 0)
        {
            canvas.interactable = false;
            fadingOut = false;
        }
        if (fadingIn && canvas.alpha < 1)
        {
            canvas.alpha = Mathf.MoveTowards(canvas.alpha, 1, fadeSpeed);
        }
        if (canvas.alpha == 1)
        {
            fadingIn = false;
            canvas.interactable = true;
        }


    }
    public void fadeIn()
    {
        fadingIn = true;
    }
    public void fadeOut()
    {
        fadingOut = true;
    }
}
