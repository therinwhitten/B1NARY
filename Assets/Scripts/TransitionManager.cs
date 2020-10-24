using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TransitionManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static TransitionManager instance;
    public RawImage overlayImage;
    public Material transitionMatPrefab;
    public Texture2D texIn;
    public Texture2D texOut;

    public VideoPlayer animatedBG;


    private void Awake()
    {
        instance = this;
        overlayImage.material = new Material(transitionMatPrefab);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    static bool sceneVisible = true;
    public static void ShowScene(bool show, float speed = 1, bool smooth = false, Texture2D transitionEffect = null)
    {
        if (transitioningOverlay != null)
            instance.StopCoroutine(transitioningOverlay);

        sceneVisible = show;

        if (transitionEffect != null)
            instance.overlayImage.material.SetTexture("_AlphaTex", transitionEffect);
        transitioningOverlay = instance.StartCoroutine(TransitioningOverlay(show, speed, smooth));

    }

    static Coroutine transitioningOverlay = null;
    static IEnumerator TransitioningOverlay(bool show, float speed, bool smooth)
    {
        float targetVal = show ? 1 : 0;
        float currentVal = instance.overlayImage.material.GetFloat("_Cutoff");
        while (currentVal != targetVal)
        {
            currentVal = smooth ? Mathf.Lerp(currentVal, targetVal, speed * Time.deltaTime) : Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningOverlay = null;
    }

    // the function we actually call
    public static void TransitionBG(string newBG, float speed = 10)
    {
        instance.overlayImage.material.SetFloat("_Cutoff", 1);
        instance.overlayImage.material.SetTexture("_AlphaTex", instance.texIn);

        transitioningBG = instance.StartCoroutine(TransitioningBG(newBG));
    }
    public static Coroutine transitioningBG = null;
    static IEnumerator TransitioningBG(string newBG, float speed = 1)
    {
        float targetVal = 0;
        float currentVal = instance.overlayImage.material.GetFloat("_Cutoff"); //should be 1
        // pull in overlay
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }

        // change background clip
        VideoClip newClip = Resources.Load<VideoClip>("Backgrounds/" + newBG);
        instance.animatedBG.clip = newClip;
        instance.animatedBG.Play();

        // change overlay and pull it out
        instance.overlayImage.material.SetTexture("_AlphaTex", instance.texOut);
        targetVal = 1;
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningBG = null;
    }
}
