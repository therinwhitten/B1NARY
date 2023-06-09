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
	using UnityEngine.Audio;
	using System.Globalization;

	public sealed class CharacterManager : Singleton<CharacterManager>
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
				Character character = Instance.SummonCharacter(gameObjectName); 
				character.controller.HorizontalPosition = float.Parse(positionRaw);
				character.ChangeCharacterName(characterName);
			}),
			["spawnchar"] = (Action<string, string>)((gameObjectName, positionRaw) =>
			{
				Character character = Instance.SummonCharacter(gameObjectName);
				character.controller.HorizontalPosition = float.Parse(positionRaw);
			}),
			["spawnempty"] = (Action<string>)(characterName =>
			{
				EmptyActor.AddTo(Instance, characterName);
			}),
			["spawnempty"] = (Action<string, string>)((characterName, voiceName) =>
			{
				Character emptyCharacter = EmptyActor.AddTo(Instance, characterName);
				emptyCharacter.controller.Mouths[0].CurrentGroup =
					Instance.voiceGroup.audioMixer.FindMatchingGroups(voiceName).Single();
			}),
			["anim"] = (Action<string, string>)((characterName, animationName) =>
			{
				Instance.GetCharacter(characterName).controller.CurrentAnimation = animationName;
			}),
			["anim"] = (Action<string>)((animationName) =>
			{
				Instance.ActiveCharacter.Value.controller.CurrentAnimation = animationName;
			}),
			["movechar"] = (Action<string, string>)((characterName, positionRaw) =>
			{
				Instance.GetCharacter(characterName).controller.SetPositionOverTime(float.Parse(positionRaw), 0.3f);
			}),
			["movechar"] = (Action<string>)((positionRaw) =>
			{
				Instance.ActiveCharacter.Value.controller.SetPositionOverTime(float.Parse(positionRaw), 0.3f);
			}),
			["movechar"] = (Action<string, string, string>)((characterName, positionRaw, time) =>
			{
				float timeParsed = float.Parse(time);
				if (timeParsed == 0f)
					Instance.GetCharacter(characterName).controller.HorizontalPosition = float.Parse(positionRaw);
				else
					Instance.GetCharacter(characterName).controller.SetPositionOverTime(float.Parse(positionRaw), timeParsed);
			}),
			["emptyscene"] = (Action)(() =>
			{
				Instance.ClearAllCharacters();
			}),
			["disablechar"] = (Action<string>)(charName =>
			{
				Instance.DisableCharacter(charName);
			}),
			["disablechar"] = (Action)(() =>
			{
				Instance.DisableCharacter(Instance.ActiveCharacter.Value.controller.CharacterName);
			}),
			["changename"] = (Action<string, string>)((oldName, newName) =>
			{
				Instance.RenameCharacter(oldName, newName);
			}),
			["changename"] = (Action<string>)((newName) =>
			{
				Instance.RenameCharacter(Instance.ActiveCharacter.Value.controller.CharacterName, newName);
			}),
		};


		public Transform Transform { get; private set; }
		public AudioMixerGroup voiceGroup;

		protected override void SingletonAwake()
		{
			Transform = GetComponent<Transform>();
		}

		/// <summary>
		/// The currently active character. <see langword="null"/> if there is no
		/// active character.
		/// </summary>
		public Character? ActiveCharacter
		{
			get => m_active;
			set
			{
				if (value.HasValue)
					value.Value.controller.Selected = true;
				if (m_active.HasValue)
					m_active.Value.controller.Selected = false;
				m_active = value;
				ActiveCharacterChanged?.Invoke(value);
			}
		}
		private Character? m_active;
		public event Action<Character?> ActiveCharacterChanged;

		public IReadOnlyDictionary<string, Character> CharactersInScene => m_charactersInScene;
		public Character GetCharacter(string name)
		{
			if (CharactersInScene.TryGetValue(name, out var output))
				return output;
			throw new MissingMemberException($"'{name}' is not a character present in the current game!");
		}
		private readonly Dictionary<string, Character> m_charactersInScene = new Dictionary<string, Character>(); 

		public bool ChangeActiveCharacterViaCharacterName(string name)
		{
			if (!CharactersInScene.TryGetValue(name, out Character character))
				return false;
			ActiveCharacter = character;
			return true;
		}
		/// <summary>
		/// Summons a character from memory via the scene, or the resources folder.
		/// </summary>
		/// <param name="gameObjectName"> The gameobject name to summon. </param>
		/// <returns> The successfully summoned character </returns>
		/// <exception cref="MissingReferenceException"/>
		public Character SummonCharacter(string gameObjectName)
		{
			Transform charTransform = Transform.Find(gameObjectName);
			if (charTransform == null)
			{
				// Try again and load via resources.
				GameObject gameObject = Resources.Load<GameObject>(prefabsPath + gameObjectName);
				if (gameObject == null)
					throw new MissingReferenceException($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, and unable to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}, most likely missing.");

				// Found successfully, adding to dictionary.
				if (AddCharacterToDictionary(Instantiate(gameObject, Transform), out Character character))
				{
					Debug.LogWarning($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, trying to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}'.\nKeep in mind summoning a " +
						"character takes alot of processing power! use command " +
						"'summonchar' to explicitly say to get it from a prefab!");
					return character;
				}

				throw new MissingReferenceException($"Gameobject or character named" +
					$"'{gameObjectName}' does not contain an actor component.");
			}
			// Successfully got from transform
			GameObject childObject = charTransform.gameObject;
			childObject.SetActive(true);
			if (AddCharacterToDictionary(childObject, out Character character1))
				return character1;
			throw new MissingReferenceException($"Gameobject or character named" +
				$"'{gameObjectName}' does not contain an actor component.");
		}
		/// <summary>
		/// Adds a character to the dictionary.
		/// </summary>
		/// <param name="gameObject"> The gameobject or character instance. </param>
		/// <param name="character"> The character, now registered in the dictionary. </param>
		/// <returns> 
		/// If the character has been sucessfully added becuase it contains an
		/// <see cref="IActor"/> component.
		/// </returns>
		public bool AddCharacterToDictionary(GameObject gameObject, out Character character)
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] is IActor controller)
				{
					character = new Character(this, gameObject, controller);
					gameObject.transform.SetParent(Transform);
					m_charactersInScene.Add(controller.CharacterName, character);
					return true;
				}
			}
			character = default;
			return false;
		}
		public bool RenameCharacter(string oldName, string newName)
		{
			if (!m_charactersInScene.TryGetValue(oldName, out Character character))
				return false;
			character.controller.CharacterName = newName;
			m_charactersInScene.Remove(oldName);
			m_charactersInScene.Add(newName, character);
			return true;
		}
		public bool DisableCharacter(string name)
		{
			if (m_charactersInScene.TryGetValue(name, out var character))
			{
				character.characterObject.SetActive(false);
				m_charactersInScene.Remove(name);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Clears all characters from the scene.
		/// </summary>
		public void ClearAllCharacters()
		{
			string[] keys = m_charactersInScene.Keys.ToArray();
			for (int i = 0; i < m_charactersInScene.Count; i++)
				DisableCharacter(keys[i]);
		}
	}

	public struct Character
	{
		public GameObject characterObject;
		public IActor controller;

		private CharacterManager manager;
		public Character(CharacterManager manager, GameObject characterObj, IActor controller)
		{
			this.characterObject = characterObj;
			this.controller = controller;
			this.manager = manager;
		}
		public void ChangeCharacterName(string newName)
		{
			manager.RenameCharacter(controller.CharacterName, newName);
		}
	}
}