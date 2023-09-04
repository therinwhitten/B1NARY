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
	using B1NARY.UI.Globalization;
	using B1NARY.DataPersistence;

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
		public static readonly CommandArray Commands = new()
		{
			["spawnchar"] = (Action<string, string, string>)((gameObjectName, positionRaw, characterName) =>
			{
				// Switching names really, quirks n stuff
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = characterName;
					return;
				}
				// Actual code
				Character character = Instance.SummonCharacter(gameObjectName);
				Vector2 pos = character.controller.ScreenPosition;
				pos.x = float.Parse(positionRaw);
				character.controller.ScreenPosition = pos;
				character.ChangeCharacterName(characterName);
			}),
			["spawnchar"] = (Action<string, string, string, string>)((gameObjectName, positionX, positionY, characterName) =>
			{
				// Switching names really, quirks n stuff
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = characterName;
					return;
				}
				// Actual code
				Character character = Instance.SummonCharacter(gameObjectName);
				Vector2 pos = character.controller.ScreenPosition;
				pos.x = float.Parse(positionX);
				pos.y = float.Parse(positionY);
				character.controller.ScreenPosition = pos;
				character.ChangeCharacterName(characterName);
			}),
			["spawnchar"] = (Action<string, string>)((gameObjectName, positionRaw) =>
			{
				if (CharacterNames.ChangingNames)
					return;
				Character character = Instance.SummonCharacter(gameObjectName);
				Vector2 pos = character.controller.ScreenPosition;
				pos.x = float.Parse(positionRaw);
				character.controller.ScreenPosition = pos;
			}),
			["spawnempty"] = (Action<string>)(characterName =>
			{
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = characterName;
					return;
				}
				Character actor = EmptyActor.AddTo(Instance);
				actor.ChangeCharacterName(characterName);
			}),
			["spawnempty"] = (Action<string, string>)((characterName, voiceName) =>
			{
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = characterName;
					return;
				}
				Character emptyCharacter = EmptyActor.AddTo(Instance);
				emptyCharacter.ChangeCharacterName(characterName);
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
			["movechar"] = (Action<string, string, string>)((characterName, positionX, positionY) =>
			{
				Instance.GetCharacter(characterName).controller.SetPositionOverTime(
					new Vector2(float.Parse(positionX), float.Parse(positionY)), 0.3f);
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
				Instance.DisableCharacter(Instance.ActiveCharacter.Value.controller.CharacterNames.CurrentName);
			}),
			["changename"] = (Action<string, string>)((oldName, newName) =>
			{
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = newName;
					return;
				}
				Instance.RenameCharacter(oldName, newName);
			}),
			["changename"] = (Action<string>)((newName) =>
			{
				if (CharacterNames.ChangingNames)
				{
					CharacterNames.ChangingNameOf.Name = newName;
					return;
				}
				Instance.RenameCharacter(Instance.ActiveCharacter.Value.controller.CharacterNames.CurrentName, newName);
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

		public IReadOnlyList<Character> CharactersInScene => m_charactersInScene;
		public Character GetCharacter(string name)
		{
			for (int i = 0; i < CharactersInScene.Count; i++)
				if (CharactersInScene[i].controller.CharacterNames.CurrentName == name)
					return CharactersInScene[i];
			throw new MissingMemberException($"'{name}' is not a character present in the current game!");
		}
		private readonly List<Character> m_charactersInScene = new(); 

		public void ChangeActiveCharacterViaCharacterName(string name)
		{
			Character character = GetCharacter(name);
			ActiveCharacter = character;
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
				if (AddNewCharacter(Instantiate(gameObject, Transform), out Character character))
				{
					Debug.LogWarning($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, trying to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}'.\nKeep in mind summoning a " +
						"character takes alot of processing power! use command " +
						"'summonchar' to explicitly say to get it from a prefab!");
					return character;
				}

				throw new MissingReferenceException($"Gameobject or character named " +
					$"'{gameObjectName}' does not contain an actor component.");
			}
			// Successfully got from transform
			GameObject childObject = charTransform.gameObject;
			childObject.SetActive(true);
			if (AddNewCharacter(childObject, out Character character1))
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
		public bool AddNewCharacter(GameObject gameObject, out Character character)
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] is IActor controller)
				{
					character = new Character(gameObject, controller);
					gameObject.transform.SetParent(Transform);
					m_charactersInScene.Add(character);
					return true;
				}
			}
			character = default;
			return false;
		}
		public void RenameCharacter(string oldName, string newName)
		{
			//bool selectedCharacter = ActiveCharacter.HasValue && ActiveCharacter.Value.controller.CharacterNames.CurrentName == oldName;
			Character character = GetCharacter(oldName);
			character.controller.CharacterNames.CurrentName = newName;
		}
		public bool DisableCharacter(string name)
		{
			for (int i = 0; i < m_charactersInScene.Count; i++)
			{
				if (m_charactersInScene[i].controller.CharacterNames.CurrentName != name)
					continue;
				m_charactersInScene[i].characterObject.SetActive(false);
				m_charactersInScene.RemoveAt(i);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Clears all characters from the scene.
		/// </summary>
		public void ClearAllCharacters()
		{
			for (int i = 0; i < m_charactersInScene.Count; i++)
				m_charactersInScene[i].characterObject.SetActive(false);
			m_charactersInScene.Clear();
		}
	}

	public struct Character
	{
		public GameObject characterObject;
		public IActor controller;
		public Character(GameObject characterObj, IActor controller)
		{
			this.characterObject = characterObj;
			this.controller = controller;
		}
		public readonly void ChangeCharacterName(string newName) => controller.CharacterNames.CurrentName = newName;
	}
}