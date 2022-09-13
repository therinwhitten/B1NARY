namespace B1NARY
{
	/*
	using System.Collections.Generic;
	using UnityEngine;
	using B1NARY.DesignPatterns;
	using B1NARY.UI;
	using System;

	public class CharacterManager : SingletonAlt<CharacterManager>
	{
		public static readonly IReadOnlyDictionary<string, Delegate> CharacterDelegateCommands = new Dictionary<string, Delegate>()
		{
			["spawnchar"] = (Action<string, string, string>)((prefabName, positionRaw, characterName) =>
			{
				Instance.SummonCharacter(prefabName, positionRaw, characterName);
			}),
			["anim"] = (Action<string, string>)((characterName, animationName) =>
			{
				Instance.changeAnimation(characterName, animationName);
			}),
			["movechar"] = (Action<string, string>)((characterName, positionRaw) =>
			{
				Instance.moveCharacter(characterName, positionRaw);
			}),
			["emptyscene"] = (Action)(() =>
			{
				Instance.emptyScene();
			}),
			["changename"] = (Action<string, string>)((oldName, newName) =>
			{
				Instance.changeName(oldName, newName);
			}),
		};

		static string prefabsPath = "Characters/Prefabs/";
		private GameObject characterLayer;

		public Dictionary<string, GameObject> charactersInScene =
			new Dictionary<string, GameObject>(5);

		protected override void SingletonAwake()
		{
			characterLayer = gameObject;
			charactersInScene.Clear();
			DontDestroyOnLoad(gameObject);
		}





		public void changeExpression(string charName, string exrpName)
		{
			GameObject character;
			charactersInScene.TryGetValue(charName, out character);
			character.GetComponent<CharScriptOld>().changeExpression(exrpName);
		}

		// moves a character horizontally to a position.
		public void moveCharacter(string charName, string positionRaw)
		{
			GameObject character;
			charactersInScene.TryGetValue(charName, out character);

			positionRaw = positionRaw.Trim();
			float positionx = float.Parse(positionRaw);

			Vector2 targetPosition = new Vector2(positionx, 0);
			character.GetComponent<CharScriptOld>().MoveTo(targetPosition, 5, true);
		}

		public void changeLightingFocus()
		{
			foreach (string key in Instance.charactersInScene.Keys)
			{
				GameObject obj;
				Instance.charactersInScene.TryGetValue(key, out obj);
				CharScriptOld script = obj.GetComponent<CharScriptOld>();

				if (script.focused)
				{
					if (key.Trim().Equals(DialogueSystem.Instance.CurrentSpeaker.Trim()))
					{
						// scriptName.lightingIntoFocus();
						continue;
					}
					else
					{
						script.lightingOutOfFocus();
					}
				}
				else
				{
					if (key.Trim().Equals(DialogueSystem.Instance.CurrentSpeaker.Trim()))
					{
						script.lightingIntoFocus();
					}
					else
					{
						// scriptName.lightingOutOfFocus();
						continue;
					}
				}

			}
		}

		public void changeName(string oldName, string newName)
		{
			GameObject character = null;
			charactersInScene.TryGetValue(oldName, out character);

			if (character != null)
			{
				charactersInScene.Remove(oldName);
				charactersInScene.Add(newName, character);
			}
		}
	}
	*/
}