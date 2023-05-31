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
				Character? character = Instance.SummonCharacter(gameObjectName);
				if (!character.HasValue)
					return;
				character.Value.controller.HorizontalPosition = float.Parse(positionRaw);
				character.Value.ChangeCharacterName(characterName);
			}),
			["spawnchar"] = (Action<string, string>)((gameObjectName, positionRaw) =>
			{
				Character? character = Instance.SummonCharacter(gameObjectName);
				if (!character.HasValue)
					return;
				character.Value.controller.HorizontalPosition = float.Parse(positionRaw);
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
				if (Instance.CharactersInScene.TryGetValue(characterName, out Character character))
					character.controller.CurrentAnimation = animationName;
				else
					Debug.LogError($"{characterName} does not exist!", Instance);
			}),
			["anim"] = (Action<string>)((animationName) =>
			{
				Instance.ActiveCharacter.Value.controller.CurrentAnimation = animationName;
			}),
			["movechar"] = (Action<string, string>)((characterName, positionRaw) =>
			{
				if (Instance.CharactersInScene.TryGetValue(characterName, out Character character))
					character.controller.SetPositionOverTime(float.Parse(positionRaw), 0.3f);
				else
					Debug.LogError($"{characterName} does not exist!", Instance);
			}),
			["movechar"] = (Action<string>)((positionRaw) =>
			{
				Instance.ActiveCharacter.Value.controller.SetPositionOverTime(float.Parse(positionRaw), 0.3f);
			}),
			["movechar"] = (Action<string, string, string>)((characterName, positionRaw, time) =>
			{
				if (Instance.CharactersInScene.TryGetValue(characterName, out Character character))
				{
					float timeParsed = float.Parse(time);
					if (timeParsed == 0f)
						character.controller.HorizontalPosition = float.Parse(positionRaw);
					else
						character.controller.SetPositionOverTime(float.Parse(positionRaw), timeParsed);
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

		public IReadOnlyDictionary<string, Character> CharactersInScene
			=> m_charactersInScene;
		private readonly Dictionary<string, Character> m_charactersInScene = new Dictionary<string, Character>(); 

		public bool ChangeActiveCharacterViaCharacterName(string name)
		{
			if (!CharactersInScene.TryGetValue(name, out Character character))
				return false;
			ActiveCharacter = character;
			return true;
		}
		public Character? SummonCharacter(string gameObjectName)
		{
			Transform charTransform = Transform.Find(gameObjectName);
			if (charTransform == null)
			{
				GameObject gameObject = Resources.Load<GameObject>(prefabsPath + gameObjectName);
				if (gameObject == null)
				{
					Debug.LogError($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, and unable to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}, most likely missing.");
					return null;
				}

				if (AddCharacterToDictionary(Instantiate(gameObject, Transform), out var character))
				{
					Debug.LogWarning($"GameObject or Character named '{gameObjectName}'" +
						" is not found in the scene, trying to summmon in Resources Folder " +
						$"'{prefabsPath}{gameObjectName}'.\nKeep in mind summoning a " +
						"character takes alot of processing power! use command " +
						"'summonchar' to explicitly say to get it from a prefab!");
					return character;
				}
			}
			GameObject childObject = charTransform.gameObject;
			childObject.SetActive(true);
			if (AddCharacterToDictionary(childObject, out var character1))
				return character1;
			return null;
		}
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