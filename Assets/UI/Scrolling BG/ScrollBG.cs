using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBG : MonoBehaviour
{
    public float speed = 5f;
    private RectTransform BGPattern;
    // Start is called before the first frame update
    void Start()
    {
        BGPattern = transform.GetChild(0).GetComponent<RectTransform>();
        Debug.Log(BGPattern.name);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = BGPattern.anchoredPosition * 100;
        BGPattern.anchoredPosition = Vector3.MoveTowards(BGPattern.anchoredPosition, targetPos, speed * Time.deltaTime);
    }
}
