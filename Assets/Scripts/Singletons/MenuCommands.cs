using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MenuCommands : Singleton<MenuCommands>
{
	// TEMP VARIABLES UNTIL I IMPLEMENT A SAVE/LOAD SYSTEM
	public string startingScript;
	public string startingScene;
	public string[] scenes;
	public List<string> scripts;
	GameObject[] panels;

	public GameObject mainPanel;
	public GameObject scriptTesting;
	private string UIPrefabsPath = "UIPrefabs";
	// Start is called before the first frame update
	void Start()
	{
		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
		scenes = new string[sceneCount];
		for (int i = 0; i < sceneCount; i++)
		{
			scenes[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
		}
		// This is so fucking retarded but I'm too lazy to figure out
		// why it won't let me just modify the value at the current index so I need
		// to just build a new array. Sue me
		string path = Application.streamingAssetsPath + "/Docs/";
		List<string> scriptsRaw = new List<string>(Directory.GetFiles(path));
		scripts = new List<string>();

		for (int i = 0; i < scriptsRaw.Count; i++)
		{
			if (!scriptsRaw[i].Contains(".meta"))
			{
				scripts.Add(Path.GetFileNameWithoutExtension(scriptsRaw[i]));
			}
		}

	}
	public void StartGame()
	{
		TransitionManager.transitionScene(startingScene);
		waitThenDO(() =>
		{
			DialogueSystem.Instance.initialize();
			ScriptParser.Instance.scriptName = startingScript;
			ScriptParser.Instance.initialize();
		});
	}

	public void ChangePanel(string panelName)
	{

	}

	public void changeToScriptTesting()
	{
		mainPanel.SetActive(false);
		scriptTesting.SetActive(true);
		Transform scriptPanel = scriptTesting.transform.Find("Scripts");
		foreach (string script in scripts)
		{
			Button scriptButton = Instantiate(Resources.Load<Button>(UIPrefabsPath + "/Testing/SelectorButton"), scriptPanel);
			scriptButton.GetComponent<SelectorButton>().value = script;
			scriptButton.GetComponentInChildren<TextMeshProUGUI>().text = script;
			scriptButton.onClick.AddListener(() => scriptButton.GetComponent<SelectorButton>().selectScriptFile());
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(scriptPanel.GetComponent<RectTransform>());

		Transform scenePanel = scriptTesting.transform.Find("Scenes");
		foreach (string scene in scenes)
		{
			Button sceneButton = Instantiate(Resources.Load<Button>(UIPrefabsPath + "/Testing/SelectorButton"), scenePanel);
			sceneButton.GetComponent<SelectorButton>().value = scene;
			sceneButton.GetComponentInChildren<TextMeshProUGUI>().text = scene;
			sceneButton.onClick.AddListener(() => sceneButton.GetComponent<SelectorButton>().selectSceneFile());
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(scenePanel.GetComponent<RectTransform>());

	}

	// Update is called once per frame
	void Update()
	{

	}
	private void waitThenDO(System.Action action)
	{
		StartCoroutine(waitForTransitionsThenDo(action));
	}
	IEnumerator waitForTransitionsThenDo(System.Action action)
	{
		while (!TransitionManager.Instance.commandsAllowed)
		{
			yield return new WaitForEndOfFrame();
		}
		action();
	}
	public override void initialize()
	{
		// throw new System.NotImplementedException();
	}
}
