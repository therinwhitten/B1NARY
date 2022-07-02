using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
public class PersistentData : Singleton<PersistentData>
{
	// path of the save directory
	string path;
	public GameState state;
	// Start is called before the first frame update
	void Awake()
	{
		initialize();
	}
	public override void initialize()
	{
		path = Application.persistentDataPath + "/Saves";
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		string[] filenames = Directory.GetFiles(path);
		// if (filenames.Length == 0)
		state = new GameState();
	}

	public void SaveGame()
	{
		// get state object
		// state = new GameState();
		state.captureState();

		// serialize state object
		// Odd's comment, this is fucking genius
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(path + "/Quicksave.sav", FileMode.Create);

		formatter.Serialize(stream, state);
		stream.Close();
		Debug.Log("Game Saved!");
		// save serialized object to file
	}

	public void LoadGame()
	{
		// pause script
		// load save file
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(path + "/Quicksave.sav", FileMode.Open);
		// read state object
		state = formatter.Deserialize(stream) as GameState;
		stream.Close();
		// reconstruct game state based on state object
		TransitionManager.transitionScene(state.scene);
		Coroutine stateLoader = Instance.StartCoroutine(loadStateAsync());
		// CharacterManager.Instance
		// unpause script
	}
	IEnumerator loadStateAsync()
	{
		while (!TransitionManager.Instance.commandsAllowed)
		{
			yield return new WaitForEndOfFrame();
		}
		CharacterManager.Instance.emptyScene();
		foreach (GameState.CharacterSnapshot character in state.characters)
		{
			GameObject gameObject = CharacterManager.Instance.spawnCharacter(
				character.prefabName,
				character.positionX.ToString(),
				character.name
			);
			CharacterManager.Instance.changeExpression(character.name, character.expression);
			CharacterManager.Instance.changeAnimation(character.name, character.animation);
			// CharacterManager.Instance.moveCharacter(character.name, character.positionX.ToString());
		}
		DialogueSystem.Instance.additiveTextEnabled = false;
		DialogueSystem.Instance.Say("");
		// DialogueSystem.Instance.Say(state.textBoxContent);
		DialogueSystem.Instance.additiveTextEnabled = state.additiveTextEnabled;
		ScriptParser.Instance.ChangeScriptFile(state.script, state.index + 2);
		foreach (SoundCoroutine soundName in state.audioSounds)
		{
			soundName.AudioSource = AudioHandler.Instance.gameObject.AddComponent<AudioSource>();
			soundName.monoBehaviour = AudioHandler.Instance;
			AudioHandler.Instance.SoundCoroutineCache.Add(soundName.AudioClip, soundName);//.PlaySound(soundName);
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			SaveGame();
		}
		if (Input.GetKeyDown(KeyCode.F8))
		{
			LoadGame();
		}
	}
}
