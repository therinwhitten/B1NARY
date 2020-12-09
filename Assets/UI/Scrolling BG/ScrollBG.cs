using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBG : MonoBehaviour
{
    public float speed = 5f;
    private RectTransform BGPattern;
    Vector2 initialPos, targetPos;
    // Start is called before the first frame update
    void Start()
    {
        BGPattern = transform.GetChild(0).GetComponent<RectTransform>();
        Debug.Log(BGPattern.name);
        initialPos = BGPattern.anchoredPosition;
        targetPos = new Vector2(BGPattern.anchoredPosition.x - BGPattern.rect.width, BGPattern.anchoredPosition.y - BGPattern.rect.height);

    }

    // Update is called once per frame
    void Update()
    {
        if (BGPattern.anchoredPosition == targetPos)
        {
            BGPattern.anchoredPosition = initialPos;
        }
        BGPattern.anchoredPosition = Vector3.MoveTowards(BGPattern.anchoredPosition, targetPos, speed * Time.deltaTime);
    }
}
