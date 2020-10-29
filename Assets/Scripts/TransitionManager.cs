using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class TransitionManager : Singleton<TransitionManager>
{
    // Start is called before the first frame update
    public RawImage overlayImage;
    public Material transitionMatPrefab;
    public Texture2D texIn;
    public Texture2D texOut;

    public string transitionColor = "pitchBlack";

    public VideoPlayer animatedBG;
    public Image staticBG;
    public bool commandsAllowed = true;

    private void Awake()
    {
        initialize();
    }
    void Start()
    {
    }

    public override void initialize()
    {
        transitionMatPrefab = Resources.Load<Material>("Transitions/TransitionEffects/TransSoft");
        Texture transitionColorTex = Resources.Load<Texture>("Transitions/Colors/" + transitionColor);
        GameObject UILayer = GameObject.Find("UI");
        GameObject panel = UILayer.transform.Find("OverlayPanel").gameObject;
        panel.SetActive(true);
        overlayImage = GameObject.Find("OverlayPanel").GetComponent<RawImage>();
        overlayImage.material = new Material(transitionMatPrefab);
        overlayImage.texture = transitionColorTex;

        GameObject bgCanvas = GameObject.Find("BG-Canvas");
        animatedBG = bgCanvas.GetComponentInChildren<VideoPlayer>();
        staticBG = bgCanvas.GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    static bool sceneVisible = true;
    public static void ShowScene(bool show, float speed = 1, bool smooth = false, Texture2D transitionEffect = null)
    {
        if (transitioningOverlay != null)
            Instance.StopCoroutine(transitioningOverlay);

        sceneVisible = show;

        if (transitionEffect != null)
            Instance.overlayImage.material.SetTexture("_AlphaTex", transitionEffect);
        transitioningOverlay = Instance.StartCoroutine(TransitioningOverlay(show, speed, smooth));

    }



    static Coroutine transitioningOverlay = null;
    static IEnumerator TransitioningOverlay(bool show, float speed, bool smooth)
    {
        float targetVal = show ? 1 : 0;
        float currentVal = Instance.overlayImage.material.GetFloat("_Cutoff");
        while (currentVal != targetVal)
        {
            currentVal = smooth ? Mathf.Lerp(currentVal, targetVal, speed * Time.deltaTime) : Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            Instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningOverlay = null;
    }


    // ************************BACKGROUND TRANSITIONS**************************


    // the function we actually call
    public static void TransitionBG(string newBG, float speed = 10)
    {
        Instance.commandsAllowed = false;

        Instance.overlayImage.material.SetFloat("_Cutoff", 1);
        Instance.overlayImage.material.SetTexture("_AlphaTex", Instance.texIn);

        transitioningBG = Instance.StartCoroutine(TransitioningBG(newBG));
    }
    public static Coroutine transitioningBG = null;
    static IEnumerator TransitioningBG(string newBG, float speed = 1)
    {
        float targetVal = 0;
        float currentVal = Instance.overlayImage.material.GetFloat("_Cutoff"); //should be 1
        // pull in overlay
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            Instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        Instance.commandsAllowed = true;

        // change background clip
        VideoClip newClip = Resources.Load<VideoClip>("Backgrounds/" + newBG);
        Instance.animatedBG.clip = newClip;
        Instance.animatedBG.Play();


        // change overlay and pull it out
        Instance.overlayImage.material.SetTexture("_AlphaTex", Instance.texOut);
        targetVal = 1;
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            Instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningBG = null;
    }

    // ************************SCENE TRANSITIONS**************************
    public static Coroutine transitioningScene = null;

    public static void transitionScene(string newScene)
    {
        Instance.commandsAllowed = false;

        Instance.overlayImage.material.SetFloat("_Cutoff", 1);
        Instance.overlayImage.material.SetTexture("_AlphaTex", Instance.texIn);

        transitioningScene = Instance.StartCoroutine(TransitioningScene(newScene));
    }

    static IEnumerator TransitioningScene(string newScene, float speed = 1)
    {
        float targetVal = 0;
        float currentVal = Instance.overlayImage.material.GetFloat("_Cutoff"); //should be 1
        // pull in overlay
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            Instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        // change scene while curtains are closed
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(newScene);
        // wait for scene to get loaded
        while (!sceneLoad.isDone)
        {
            yield return null;
        }
        Instance.commandsAllowed = true;
        // re-initialize singletons
        DialogueSystem.Instance.initialize();
        // DialogueSystem.Instance.speechText.text = DialogueSystem.Instance.targetSpeech;
        ScriptParser.Instance.initialize();
        CharacterManager.Instance.initialize();
        CommandsManager.Instance.initialize();
        Instance.initialize();
        // pull off overlay
        Instance.overlayImage.material.SetTexture("_AlphaTex", Instance.texOut);
        targetVal = 1;
        while (targetVal != currentVal)
        {
            currentVal = Mathf.MoveTowards(currentVal, targetVal, speed * Time.deltaTime);
            Instance.overlayImage.material.SetFloat("_Cutoff", currentVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningScene = null;
    }


}
