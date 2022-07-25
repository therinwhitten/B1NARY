using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class GameState
{
	public static GameState LoadExistingData(string savePathWithFileNameAndExtension)
	{
		using (var stream = new FileStream(savePathWithFileNameAndExtension, FileMode.Open))
			return new BinaryFormatter().Deserialize(stream) as GameState;
	}

	private static Dictionary<string, GameObject> CharactersInScene => CharacterManager.Instance.charactersInScene;

	public Dictionary<string, string> strings;
	public Dictionary<string, bool> bools;
	public Dictionary<string, int> ints;
	public Dictionary<string, float> floats;

	public readonly string script, scene, textBoxContent;
	public readonly int index;
	public readonly bool additiveTextEnabled;
	public readonly AudioData[] audioSounds;
	public readonly CharacterSnapshot[] characterSnapshots;

	public GameState()
	{
		// Additive
		additiveTextEnabled = DialogueSystem.Instance.additiveTextEnabled;
		if (additiveTextEnabled)
			textBoxContent = DialogueSystem.Instance.targetSpeech;
		else
			textBoxContent = string.Empty;

		// Basics
		scene = SceneManager.GetActiveScene().name;
		script = ScriptParser.Instance.scriptName;
		index = ScriptParser.Instance.currentNode.index;

		// Chardata
		characterSnapshots = CharactersInScene.Values.Select(GetCharacterData).ToArray();
		CharacterSnapshot GetCharacterData(GameObject character)
		{
			var charScript = character.GetComponent<CharacterScript>();
			return new CharacterSnapshot()
			{
				name = charScript.charName,
				prefabName = charScript.prefabName,
				positionX = charScript.currentPosition.x,
				positionY = charScript.currentPosition.y,
				animation = charScript.currentAnimation,
				focused = charScript.focused
			};
		}

		// Audio
		audioSounds = AudioHandler.Instance.SoundCoroutineCache.Values
			.Select(coroutine => new AudioData(coroutine.AudioClip.Name,
			coroutine.currentSoundLibrary, coroutine.AudioSource.time)).ToArray();
	}

	public void SaveDataIntoMemory(string savePathWithFileNameAndExtension)
	{
		string directoryPath = savePathWithFileNameAndExtension.Remove(
			savePathWithFileNameAndExtension.LastIndexOfAny(new char[] { '/', '\\' }));
		if (!Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);
		using (var stream = new FileStream(savePathWithFileNameAndExtension, FileMode.Create))
			new BinaryFormatter().Serialize(stream, this);
	}

	public Task LoadDataIntoMemory()
	{
		LoadCharacters();
		LoadDialogue();
		ApplyDictionaries();
		ScriptParser.Instance.ChangeScriptFile(script, index + 2);
		LoadAudio(audioSounds);
		Debug.Log("Loaded game!");
		return Task.CompletedTask;

		void LoadCharacters()
		{
			CharacterManager.Instance.emptyScene();
			foreach (CharacterSnapshot character in characterSnapshots)
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
		}
		void LoadDialogue()
		{
			DialogueSystem.Instance.additiveTextEnabled = false;
			DialogueSystem.Instance.Say(string.Empty);
			DialogueSystem.Instance.additiveTextEnabled = additiveTextEnabled;
		}
		void LoadAudio(AudioData[] audioSounds)
		{
			const string fileDirectory = "Sounds/Sound Libraries";
			foreach (var (clip, libraryName, length) in audioSounds)
			{
				CoroutinePointer soundCoroutinePointer;
				if (AudioHandler.Instance.CustomAudioData.name != libraryName)
				{
					var library = Resources.Load<SoundLibrary>($"{fileDirectory}/{libraryName}");
					soundCoroutinePointer = AudioHandler.Instance.PlaySound(
						library.GetCustomAudioClip(library.GetAudioClip(clip)));
				}
				else
					soundCoroutinePointer = AudioHandler.Instance.PlaySound(clip);
				soundCoroutinePointer().AudioSource.time = length;
			}
		}
		void ApplyDictionaries()
		{
			PersistentData.strings = strings;
			PersistentData.bools = bools;
			PersistentData.ints = ints;
			PersistentData.floats = floats;
		}
	}

	[Serializable]
	public class CharacterSnapshot
	{
		public string name;
		public string prefabName;
		public float positionX;
		public float positionY;
		public string expression;
		public string animation;
		public bool focused;
	}

	[Serializable]
	public struct AudioData
	{
		public static explicit operator AudioData((string soundName, string soundLibrary, float currentPoint) inputTuple)
			=> new AudioData(inputTuple.soundName, inputTuple.soundLibrary, inputTuple.currentPoint);

		public readonly string soundName, soundLibrary;
		public readonly float currentPoint;
		public AudioData(string soundName, string soundLibrary, float currentPoint)
		{
			this.soundName = soundName;
			this.soundLibrary = soundLibrary;
			this.currentPoint = currentPoint;
		}
		public void Deconstruct(out string soundName, out string soundLibrary, out float currentPoint)
		{
			soundName = this.soundName;
			soundLibrary = this.soundLibrary;
			currentPoint = this.currentPoint;
		}
	}
}
