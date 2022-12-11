namespace B1NARY.CharacterManagement
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;
	using B1NARY.UI;
	using B1NARY.Scripting;

	public sealed class CharacterController : Singleton<CharacterController>
	{
		/// <summary>
		/// The preset path for using <see cref="Resources.Load(string)"/> to 
		/// easily load in characters.
		/// </summary>
		public const string prefabsPath = "Characters/Prefabs/";

		/// <summary>
		/// Commands for <see cref="ScriptHandler"/>.
		/// </summary>
		public static readonly CommandArray Commands = new CommandArray()
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
					" in the scene!");
			}),
			["spawnchar"] = (Action<string, string>)((gameObjectName, positionRaw) =>
			{
				if (Instance.InitiateCharacter(gameObjectName, positionRaw, gameObjectName))
					return;
				if (Instance.SummonCharacter(gameObjectName, positionRaw, gameObjectName))
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
					" in the scene!");
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
			["spawnempty"] = (Action<string>)(characterName =>
			{
				var pair = EmptyController.Instantiate(Instance.transform, characterName);
				Instance.charactersInScene.Add(characterName, pair);
			}),
			["anim"] = (Action<string, string>)((characterName, animationName) =>
			{
				if (Instance.charactersInScene.TryGetValue(characterName, out var pair))
					pair.characterScript.CurrentAnimation = animationName;
				else
					Debug.LogError($"{characterName} does not exist!", Instance);
			}),
			["movechar"] = (Action<string, string>)((characterName, positionRaw) =>
			{
				if (Instance.charactersInScene.TryGetValue(characterName, out var pair))
					pair.characterScript.SetPositionOverTime(float.Parse(positionRaw), 0.3f);
				else
					Debug.LogError($"{characterName} does not exist!", Instance);
			}),
			["movechar"] = (Action<string, string, string>)((characterName, positionRaw, time) =>
			{
				if (Instance.charactersInScene.TryGetValue(characterName, out var pair))
				{
					float timeParsed = float.Parse(time);
					if (timeParsed == 0f)
						pair.characterScript.HorizontalPosition = float.Parse(positionRaw);
					else
						pair.characterScript.SetPositionOverTime(float.Parse(positionRaw), timeParsed);
				}
				else
					Debug.LogError($"{characterName} does not exist!", Instance);
			}),
			["emptyscene"] = (Action)(() =>
			{
				Instance.ClearAllCharacters();
			}),
			["disablechar"] = (Action<string>)(charName =>
			{
				Instance.DisableCharacter(charName);
			}),
			["changename"] = (Action<string, string>)((oldName, newName) =>
			{
				Instance.ChangeName(oldName, newName);
			}),
		};



		[SerializeField] private Canvas characterLayer;
		private Transform charLayerTransform;


		public string ActiveCharacterName { get; private set; }
		public ICharacterController ActiveCharacter => charactersInScene[ActiveCharacterName].characterScript;
		public event Action<ICharacterController> ActiveCharacterChanged;
		public Dictionary<string, (GameObject gameObject, ICharacterController characterScript)> charactersInScene =
			new Dictionary<string, (GameObject gameObject, ICharacterController characterScript)>(10);

		/// <summary>
		/// Changes the currently active character without errors. 
		/// </summary>
		/// <param name="name"> The case-sensitive name of the character. </param>
		/// <returns> If it has successfully switched characters. </returns>
		public bool ChangeActiveCharacter(string name)
		{
			if (charactersInScene.ContainsKey(name))
			{
				ActiveCharacterName = name;
				ActiveCharacterChanged?.Invoke(ActiveCharacter);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Modifies a character name for <see cref="charactersInScene"/>.
		/// </summary>
		/// <param name="oldKey"> The current name to change. </param>
		/// <param name="newKey"> The new name. </param>
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


		/// <summary>
		/// Explicitly takes a character from disabled memory onto the scene.
		/// <para>
		/// This is the text only version for scripts, for hard-coding use, see
		/// <see cref="InitiateCharacter(string, float, string)"/>.
		/// </para>
		/// </summary>
		/// <param name="gameObjectName"> The character's <see cref="GameObject"/> name. </param>
		/// <param name="positionRaw"> The new position to assign on the X axis. Text only, will be parsed as <see cref="float"/> </param>
		/// <param name="characterName"> The value to modify the character's name. </param>
		/// <returns> If the <see cref="GameObject"/> is found under <paramref name="gameObjectName"/>. </returns>
		public bool InitiateCharacter(string gameObjectName, string positionRaw, string characterName)
			=> InitiateCharacter(gameObjectName, float.Parse(positionRaw), characterName);

		/// <summary>
		/// Explicitly takes a character from disabled memory onto the scene.
		/// </summary>
		/// <param name="gameObjectName"> The character's <see cref="GameObject"/> name. </param>
		/// <param name="xPosition"> The new position to assign on the X axis. </param>
		/// <param name="characterName"> The value to modify the character's name. </param>
		/// <returns> If the <see cref="GameObject"/> is found under <paramref name="gameObjectName"/>. </returns>
		public bool InitiateCharacter(string gameObjectName, float xPosition, string characterName = "")
		{
			Transform charTransform = charLayerTransform.Find(gameObjectName);
			if (charTransform == null)
				return false;
			CharacterScript script = charTransform.GetComponent<CharacterScript>();
			InitiateCharacter(script.gameObject, script, xPosition, characterName);
			return true;
		}

		/// <summary>
		/// Base method that makes the character more manage-able in code.
		/// </summary>
		/// <param name="script"> The character's <see cref="CharacterScript"/>. </param>
		/// <param name="object"> The character's <see cref="GameObject"/>. </param>
		/// <param name="xPosition"> The new position to assign on the X axis. </param>
		/// <param name="characterName"> The value to modify the character's name. </param>
		private void InitiateCharacter(GameObject @object, CharacterScript script, float xPosition, string characterName)
		{
			@object.SetActive(true);
			ModifyGameObjectName(@object, characterName);
			script.HorizontalPosition = xPosition;
			charactersInScene.Add(script.CharacterName, (@object, script));
		}

		/// <summary>
		/// Explicitly tries to take a prefab of an existing character by combining
		/// <see cref="prefabsPath"/> and the <paramref name="gameObjectName"/>
		/// for <see cref="Resources.Load(string)"/> for use in the scene.
		/// <para>
		/// This is the text only version for scripts, for hard-coding use, see
		/// <see cref="SummonCharacter(string, float, string)"/>.
		/// </para>
		/// </summary>
		/// <param name="gameObjectName"> The character's <see cref="GameObject"/> name. </param>
		/// <param name="positionRaw"> The new position to assign on the X axis. Text only, will be parsed as <see cref="float"/> </param>
		/// <param name="characterName"> The value to modify the character's name. </param>
		/// <returns> If the character is found in the path. </returns>
		public bool SummonCharacter(string gameObjectName, string positionRaw, string characterName)
			=> SummonCharacter(gameObjectName, float.Parse(positionRaw), characterName);
		
		/// <summary>
		/// Explicitly tries to take a prefab of an existing character by combining
		/// <see cref="prefabsPath"/> and the <paramref name="gameObjectName"/>
		/// for <see cref="Resources.Load(string)"/> for use in the scene.
		/// </summary>
		/// <param name="gameObjectName"> The character's <see cref="GameObject"/> name. </param>
		/// <param name="positionRaw"> The new position to assign on the X axis. Text only, will be parsed as <see cref="float"/> </param>
		/// <param name="characterName"> The value to modify the character's name. </param>
		/// <returns> If the character is found in the path. </returns>
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

		/// <summary>
		/// Modifies the <paramref name="gameObject"/>'s name if <paramref name="characterName"/>
		/// is not <see langword="null"/> or empty.
		/// </summary>
		/// <param name="gameObject"> The GameObject to modify. </param>
		/// <param name="characterName"> The new name to assign. Does nothing if empty. </param>
		private void ModifyGameObjectName(GameObject gameObject, string characterName)
		{
			if (string.IsNullOrWhiteSpace(characterName))
				return;
			gameObject.name = characterName;
		}

		/// <summary>
		/// Clears all characters from the scene.
		/// </summary>
		public void ClearAllCharacters()
		{
			string[] keys = charactersInScene.Keys.ToArray();
			for (int i = 0; i < charactersInScene.Count; i++)
				DisableCharacter(keys[i]);
		}

		/// <summary>
		/// Removes a single character from the scene.
		/// </summary>
		public void DisableCharacter(string character)
		{
			charactersInScene[character].gameObject.SetActive(false);
			charactersInScene.Remove(character);
		}
	}
}