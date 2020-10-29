using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Expression;


public class CharacterScript : MonoBehaviour
{
    public string charName;
    public CharacterLibrary library;
    public CharacterEntry libraryEntry;
    public string[] expressions;
    public RectTransform rectTransform;

    public CharacterScript instance;

    public Vector2 anchorPadding { get { return rectTransform.anchorMax - rectTransform.anchorMin; } }

    private void Awake()
    {
        instance = this;
        rectTransform = gameObject.GetComponent<RectTransform>();
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
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     SetPosition(target);
            // }

            yield return new WaitForEndOfFrame();
        }
        StopMoving();
    }

}