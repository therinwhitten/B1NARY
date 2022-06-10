using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;

public class CharacterScript : MonoBehaviour
{
    [HideInInspector]
    public CharacterScript instance;

    [HideInInspector]
    public string currentExpression;
    [HideInInspector]
    public string prefabName;

    [HideInInspector]
    public string currentAnimation = "idle";

    private string defaultAnimation = "idle";
    public string charName;
    public string[] expressions;
    [HideInInspector]
    public RectTransform rectTransform;
    public AudioClip GetVoiceLine(int index) => ScriptParser.Instance.GetVoiceLine(index);
    public AudioSource voice;
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
            AudioHandler.Instance.VoiceActorHandler.PlayVoice(name, voicevolume, voice, GetVoiceLine(Line.index));
            // float volume = voice.volume;
            // voice.volume = 0f;
            // voice.Stop();
            // voice.clip = ScriptParser.Instance.voiceLines[Line.index.ToString()];
            // voice.volume = volume;
            // voice.Play();
        }
        catch (KeyNotFoundException ex)
        {
            {
                Debug.LogError($"Voice line not found: Line {Line.index}\n"
                    + $"{ex}\n\n {ex.TargetSite}\n\n{ex.Source}\n\n{ex.HResult}");
            }
        }
    }


    public void animate(string animName)
    {
        currentAnimation = animName;
        try
        {
            gameObject.GetComponent<Animator>().SetTrigger(animName.Trim());
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Error! Animation "
            + animName +
            " not found in animation list of character: "
            + charName +
            ". Reverting to default animation");
            gameObject.GetComponent<Animator>().SetTrigger(defaultAnimation.Trim());
            currentAnimation = defaultAnimation;
        }
    }

    public void changeExpression(string expressionName)
    {
        currentExpression = expressionName;
        int expressionIndex = System.Array.IndexOf(expressions, expressionName);
        // Debug.Log("Changing expression of character " + charName + ". Name: " + expressionName + ". Index: " + expressionIndex);
        try
        {
            gameObject.GetComponent<CubismExpressionController>().CurrentExpressionIndex = expressionIndex;
        }
        catch (System.NullReferenceException)
        {
            Debug.LogWarning("Error! Expression " + expressionName + " not found in expression list of character: " + charName);
        }
    }

    [HideInInspector]
    public Vector2 targetPosition;
    public Vector2 currentPosition;
    Coroutine moving;
    [HideInInspector]
    public bool isMoving { get { return moving != null; } }

    public void MoveTo(Vector2 Target, float speed, bool smooth = true)
    {
        StopMoving();
        moving = StartCoroutine(Moving(Target, speed, smooth));
        currentPosition = Target;
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
        // currentPosition = targetPosition;
        moving = null;
    }
    public void SetPosition(Vector2 target)
    {
        currentPosition = target;
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
        currentPosition = target;
        StopMoving();
    }

}