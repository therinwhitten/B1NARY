using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollImage : MonoBehaviour
{
    public float speed = 50f;

    private RectTransform rectTransform;
    Vector2 initialPos, targetPos;


    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Debug.Log(rectTransform.name);
        initialPos = rectTransform.anchoredPosition;
        targetPos = new Vector2(rectTransform.anchoredPosition.x - rectTransform.rect.width, rectTransform.anchoredPosition.y + rectTransform.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        if (rectTransform.anchoredPosition == targetPos)
        {
            rectTransform.anchoredPosition = initialPos;
        }
        rectTransform.anchoredPosition = Vector3.MoveTowards(rectTransform.anchoredPosition, targetPos, speed * Time.deltaTime);

    }
}
