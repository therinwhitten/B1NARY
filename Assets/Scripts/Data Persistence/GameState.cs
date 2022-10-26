namespace B1NARY.DataPersistence
{
	using B1NARY.UI;
	using B1NARY.Audio;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using B1NARY.Scripting;
	using B1NARY.DesignPatterns;

	[Serializable]
	public class GameState
	{
		public static GameState LoadExistingData(string savePathWithFileNameAndExtension)
		{
			using (var stream = new FileStream(savePathWithFileNameAndExtension, FileMode.Open))
				return new BinaryFormatter().Deserialize(stream) as GameState;
		}

		#region About
		public TimeSpan timePlayed;
		public DateTime lastSaved;
		#endregion


		public Dictionary<string, string> strings;
		public Dictionary<string, bool> bools;
		public Dictionary<string, int> ints;
		public Dictionary<string, float> floats;

		public string playerName;
		public string scene, documentPath;

		public GameState()
		{

		}
		/*
		public string playerName;
		public readonly string scriptName, scene, textBoxContent;
		public readonly ScriptLine expectedScriptLine;
		public readonly bool additiveTextEnabled;
		public readonly AudioData[] audioSounds;
		public readonly CharacterSnapshot[] characterSnapshots;

		public GameState()
		{
			// Additive
			additiveTextEnabled = DialogueSystem.Instance.AdditiveTextEnabled;
			if (additiveTextEnabled)
				textBoxContent = DialogueSystem.Instance.CurrentText;
			else
				textBoxContent = string.Empty;

			// Basics
			scene = SceneManager.GetActiveScene().name;
			scriptName = ScriptHandler.Instance.ScriptName;
			expectedScriptLine = ScriptHandler.Instance.CurrentLine;
			//index = ScriptHandler.Instance.;

			// Chardata
			throw new NotImplementedException();
			/*
			characterSnapshots = CharactersInScene.Values.Select(GetCharacterData).ToArray();
			CharacterSnapshot GetCharacterData(GameObject character)
			{
				var charScript = character.GetComponent<CharScriptOld>();
				return new CharacterSnapshot()
				{
					name = charScript.charName,
					prefabName = charScript.prefabName,
					positionX = charScript.currentPosition.x,
					positionY = charScript.currentPosition.y,
					animation = charScript.currentAnimation,
					focused = charScript.focused
				};
			}*/

			// Audio
			//hoverSounds = AudioHandler.Instance.SoundCoroutineCache.Values
			//	.Select(coroutine => new AudioData(coroutine.AudioClip.Name,
			//	coroutine.currentSoundLibrary, coroutine.AudioSource.time)).ToArray();
	/*
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
			ScriptHandler.Instance.InitializeNewScript(scriptName);
			while (ScriptHandler.Instance.CurrentLine != expectedScriptLine)
				ScriptHandler.Instance.NextLine();
			//LoadAudio(hoverSounds);
			Debug.Log("Loaded game!");
			return Task.CompletedTask;

			void LoadCharacters()
			{
				throw new NotImplementedException();
				B1NARY.CharacterController.Instance.ClearAllCharacters();
				
				/*
				foreach (CharacterSnapshot characterSnapshot in characterSnapshots)
				{
					GameObject charObj = B1NARY.CharacterController.Instance
						.SummonCharacter(characterSnapshot.prefabName, )
				}
				/*
				foreach (CharacterSnapshot character in characterSnapshots)
				{
					GameObject gameObject = CharacterManager.Instance.SummonCharacter(
						character.prefabName,
						character.positionX.ToString(),
						character.name
					);
					CharacterManager.Instance.changeExpression(character.name, character.expression);

					CharacterManager.Instance.changeAnimation(character.name, character.animation);
					// CharacterManager.Instance.moveCharacter(character.name, character.positionX.ToString());
				}*/
			}/*
			void LoadDialogue()
			{
				DialogueSystem.Instance.AdditiveTextEnabled = false;
				DialogueSystem.Instance.Say(string.Empty);
				DialogueSystem.Instance.AdditiveTextEnabled = additiveTextEnabled;
			}
			/*
			void LoadAudio(AudioData[] hoverSounds)
			{
				const string fileDirectory = "Audio/Sound Libraries";
				foreach (var (clip, libraryName, length) in hoverSounds)
				{
					CoroutinePointer soundCoroutinePointer;
					if (AudioHandler.Instance.CustomAudioData.name != libraryName)
					{
						var library = Resources.Load<SoundLibrary>($"{fileDirectory}/{libraryName}");
						soundCoroutinePointer = AudioHandler.Instance.PlayHoverSound(
							library.GetCustomAudioClip(library.GetAudioClip(clip)));
					}
					else
						soundCoroutinePointer = AudioHandler.Instance.PlayHoverSound(clip);
					soundCoroutinePointer().AudioSource.time = length;
				}
			}
			*//*
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
			public Vector2 position;
			public string expression;
			public string animation;
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
	*/
}