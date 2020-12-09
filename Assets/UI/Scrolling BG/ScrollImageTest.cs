using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollImageTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.position;
        endPosition = startPosition * 100;
        IEnumerator cachedCoroutine = LerpObject();
        StartCoroutine(cachedCoroutine);
    }

    // Update is called once per frame
    void Update()
    {
    }
    float timeOfTravel = 5; //time after object reach a target place 
    float currentTime = 0; // actual floting time 
    float normalizedValue;
    RectTransform rectTransform; //getting reference to this component
    Vector3 startPosition, endPosition;

    IEnumerator LerpObject()
    {

        while (true)
        {
            currentTime += Time.deltaTime;
            normalizedValue = currentTime / timeOfTravel; // we normalize our time 

            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, normalizedValue);
            yield return null;
        }
    }
}
