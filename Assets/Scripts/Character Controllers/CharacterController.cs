namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;
	using B1NARY.UI;
	using B1NARY.Scripting.Experimental;

	public sealed class CharacterController : Singleton<CharacterController>
	{
		public const string prefabsPath = "Characters/Prefabs/";
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			["spawnchar"] = (Action<string, string, string>)((gameObjectName, positionRaw, characterName) =>
			{
				if (Instance.InitiateCharacter(gameObjectName, positionRaw, characterName))
					return;
				if (Instance.SummonCharacter(gameObjectName, positionRaw, characterName))
				{
					Debug.LogWarning($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, trying to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}'.\nKeep in mind summoning a " +
						"character takes alot of processing power! use command " +
						"'summonchar' to explicitly say to get it from a prefab!");
					return;
				}
				throw new MissingReferenceException("GameObject or Character " +
					$"named '{gameObjectName}' is not found as prefab in " +
					$"Resources Folder '{prefabsPath}{gameObjectName}', nor found" +
					"in the scene!");
			}),
			["initiatechar"] = (Action<string, string, string>)((gameObjectName, positionRaw, characterName) =>
			{
				if (Instance.InitiateCharacter(gameObjectName, positionRaw, characterName))
					return;
				throw new MissingReferenceException("GameObject or Character " +
					$"named '{gameObjectName}' is not found in the scene!");
			}),
			["summonchar"] = (Action<string, string, string>)((gameObjectName, positionRaw, characterName) =>
			{
				if (Instance.SummonCharacter(gameObjectName, positionRaw, characterName))
					return;
				throw new MissingReferenceException("GameObject or Character " +
					$"named '{gameObjectName}' is not found as prefab in " +
					$"Resources Folder '{prefabsPath}{gameObjectName}'!");
			}),

			["anim"] = (Action<string, string>)((characterName, animationName) =>
			{
				Instance.charactersInScene[characterName].characterScript.PlayAnimation(animationName);
			}),
			["movechar"] = (Action<string, string>)((characterName, positionRaw) =>
			{
				Instance.charactersInScene[characterName].characterScript.SetPosition(float.Parse(positionRaw));
			}),
			["emptyscene"] = (Action)(() =>
			{
				Instance.ClearAllCharacters();
			}),
			["changename"] = (Action<string, string>)((oldName, newName) =>
			{
				Instance.ChangeName(oldName, newName);
			}),
		};



		[SerializeField] private Canvas characterLayer;
		private Transform charLayerTransform;

		public Dictionary<string, (GameObject gameObject, CharacterScript characterScript)> charactersInScene =
			new Dictionary<string, (GameObject gameObject, CharacterScript characterScript)>(5);
		public void ChangeName(string oldKey, string newKey)
		{
			var pair = charactersInScene[oldKey];
			charactersInScene.Remove(oldKey);
			pair.gameObject.name = newKey;
			charactersInScene.Add(newKey, pair);
		}


		protected override void SingletonAwake()
		{
			charLayerTransform = characterLayer.transform;
		}
		public void PlayVoiceActor(ScriptLine speechLine)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			if (charactersInScene.TryGetValue(currentSpeaker, out var charObject))
				charObject.characterScript.SayLine(speechLine);
			else
				Debug.LogError($"Character '{currentSpeaker}' does not exist!");
		}

		public bool InitiateCharacter(string gameObjectName, string positionRaw, string characterName)
			=> InitiateCharacter(gameObjectName, float.Parse(positionRaw), characterName);
		public bool InitiateCharacter(string gameObjectName, float xPosition, string characterName = "")
		{
			Transform charTransform = charLayerTransform.Find(gameObjectName);
			if (charTransform == null)
				return false;
			CharacterScript script = charTransform.GetComponent<CharacterScript>();
			InitiateCharacter(script.gameObject, script, xPosition, characterName);
			return true;
		}
		private void InitiateCharacter(GameObject @object, CharacterScript script, float xPosition, string characterName)
		{
			@object.SetActive(true);
			script.CharacterName = ModifyGameObjectName(@object.name, characterName);
			charactersInScene.Add(script.CharacterName, (@object, script));
		}
		public bool SummonCharacter(string gameObjectName, string positionRaw, string characterName)
			=> SummonCharacter(gameObjectName, float.Parse(positionRaw), characterName);
		public bool SummonCharacter(string gameObjectName, float xPosition, string characterName = "")
		{
			GameObject gameObject = Resources.Load<GameObject>(prefabsPath + gameObjectName);
			if (gameObject == null)
				return false;
			if (!gameObject.TryGetComponent<CharacterScript>(out var characterScript))
				return false;
			InitiateCharacter(gameObject, characterScript, xPosition, characterName);
			return true;
		}

		private string ModifyGameObjectName(string gameObjectName, string characterName)
		{
			if (string.IsNullOrWhiteSpace(characterName))
				return gameObjectName;
			return characterName;
		}

		public void ClearAllCharacters()
		{
			string[] keys = charactersInScene.Keys.ToArray();
			for (int i = 0; i < charactersInScene.Count; i++)
			{
				Destroy(charactersInScene[keys[i]].gameObject);
				charactersInScene.Remove(keys[i]);
			}
		}
	}
}