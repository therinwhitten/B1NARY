using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;

public class CharacterScript : MonoBehaviour
{
    public string charName;
    public CharacterLibrary library;
    public CharacterEntry libraryEntry;
    public string[] expressions;
    public RectTransform rectTransform;
    Dictionary<string, AudioClip> voiceLines { get { return ScriptParser.Instance.voiceLines; } }
    public AudioSource voice;
    public CharacterScript instance;

    public CubismRenderer[] renderers;

    private Material lighting;
    private Material lightingFocus;
    private Material lightingNoFocus;
    public bool focused = false;
    public Vector2 anchorPadding { get { return rectTransform.anchorMax - rectTransform.anchorMin; } }
    private Vector3 originalScale;
    float voicevolume = 1f;
    private void Awake()
    {
        instance = this;
        rectTransform = gameObject.GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        voice = gameObject.GetComponent<AudioSource>();
        renderers = gameObject.GetComponentsInChildren<CubismRenderer>();
        initLighting();
    }
    public void lightingIntoFocus()
    {
        focused = true;
        targetMaterial = lighting;
        targetScale = new Vector3(originalScale.x * 1.05f, originalScale.y * 1.05f, originalScale.z);
        transitionFocus();
    }
    public void lightingOutOfFocus()
    {
        focused = false;
        targetMaterial = lightingNoFocus;
        targetScale = new Vector3(originalScale.x * 0.95f, originalScale.y * 0.95f, originalScale.z);
        transitionFocus();
    }

    private void stopLighting()
    {
        if (transitioningFocus != null)
        {
            StopCoroutine(transitioningFocus);
            transitioningFocus = null;
        }
    }
    private void transitionFocus()
    {
        stopLighting();
        transitioningFocus = this.StartCoroutine(TransitioningFocus());
    }
    Coroutine transitioningFocus = null;
    Material targetMaterial;
    Vector3 targetScale;
    IEnumerator TransitioningFocus()
    {
        changeLighting(targetMaterial);

        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.MoveTowards(rectTransform.localScale, targetScale, 5f);
            yield return new WaitForEndOfFrame();
        }
        transitioningFocus = null;
    }
    public void initLighting()
    {
        lighting = new Material(GameObject.Find("LightingOverlay").GetComponent<SpriteRenderer>().material);

        lightingFocus = new Material(lighting);
        lightingFocus.color = new Color(lighting.color.r + lighting.color.r / 4, lighting.color.g + lighting.color.g / 4, lighting.color.b + lighting.color.b / 4);

        lightingNoFocus = new Material(lighting);
        lightingNoFocus.color = new Color(lighting.color.r - lighting.color.r / 2, lighting.color.g - lighting.color.g / 2, lighting.color.b - lighting.color.b / 2);

        foreach (CubismRenderer renderer in renderers)
        {
            if (lighting != null)
            {
                renderer.Material = lighting;
            }
        }
    }
    private void changeLighting(Material material)
    {
        foreach (CubismRenderer renderer in renderers)
        {
            if (material != null)
            {
                renderer.Material = material;
            }
        }
    }

    public void speak(string name, DialogueLine Line)
    {
        try
        {
            // Debug.Log("Speaking line " + Line.index.ToString() + ": " + Line.line);
            AudioManager.Instance.PlayVoice(name, voicevolume, voice, voiceLines[Line.index.ToString()]);
            // float volume = voice.volume;
            // voice.volume = 0f;
            // voice.Stop();
            // voice.clip = ScriptParser.Instance.voiceLines[Line.index.ToString()];
            // voice.volume = volume;
            // voice.Play();
        }
        catch (KeyNotFoundException)
        {
            {
                Debug.LogWarning("Voice line not found: Line " + Line.index);
            }
        }
    }


    public void animate(string animName)
    {
        gameObject.GetComponent<Animator>().SetTrigger(animName.Trim());
    }

    public void changeExpression(string expressionName)
    {
        int expressionIndex = System.Array.IndexOf(expressions, expressionName);
        // Debug.Log("Changing expression of character " + charName + ". Name: " + expressionName + ". Index: " + expressionIndex);
        gameObject.GetComponent<CubismExpressionController>().CurrentExpressionIndex = expressionIndex;
    }

    Vector2 targetPosition;
    Coroutine moving;
    bool isMoving { get { return moving != null; } }

    public void MoveTo(Vector2 Target, float speed, bool smooth = true)
    {
        StopMoving();
        moving = StartCoroutine(Moving(Target, speed, smooth));
    }
    public void StopMoving(bool goToTarget = true)
    {
        if (isMoving)
        {
            StopCoroutine(moving);
            if (goToTarget)
            {
                SetPosition(targetPosition);
            }
        }
        moving = null;
    }
    public void SetPosition(Vector2 target)
    {
        Vector2 padding = anchorPadding;

        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;

        Vector2 minAnchorTarget = new Vector2(maxX * target.x, maxY * target.y);
        rectTransform.anchorMin = minAnchorTarget;
        rectTransform.anchorMax = rectTransform.anchorMin + padding;

    }
    IEnumerator Moving(Vector2 target, float speed, bool smooth)
    {
        targetPosition = target;

        Vector2 padding = anchorPadding;

        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;

        Vector2 minAnchorTarget = new Vector2(maxX * targetPosition.x, maxY * targetPosition.y);
        speed *= Time.deltaTime;

        while (rectTransform.anchorMin != minAnchorTarget)
        {
            rectTransform.anchorMin = (!smooth) ? Vector2.MoveTowards(rectTransform.anchorMin, minAnchorTarget, speed) : Vector2.Lerp(rectTransform.anchorMin, minAnchorTarget, speed);
            rectTransform.anchorMax = rectTransform.anchorMin + padding;
            yield return new WaitForEndOfFrame();
        }
        StopMoving();
    }

}