namespace B1NARY
{
	using System.Collections.Generic;
	using UnityEngine;
	using B1NARY.DesignPatterns;
	using B1NARY.UI;

	public class CharacterManager : SingletonAlt<CharacterManager>
	{
		static string prefabsPath = "Characters/Prefabs/";
		private GameObject characterLayer;

		public Dictionary<string, GameObject> charactersInScene =
			new Dictionary<string, GameObject>(5);

		protected override void SingletonAwake()
		{
			characterLayer = gameObject;
			charactersInScene.Clear();
		}



		public GameObject SummonCharacter(string prefabName, string positionRaw, string charName = "")
		{
			Transform transform = characterLayer.transform;
			GameObject characterObject = Instantiate(Resources.Load<GameObject>(prefabsPath + prefabName), transform);
			characterObject.SetActive(true);
			characterObject.transform.position = new Vector3(transform.position.x, characterObject.transform.position.y, characterObject.transform.position.z);
			characterObject.GetComponent<CharacterScript>().SetPosition(new Vector2(float.Parse(positionRaw), 0));
			characterObject.GetComponent<CharacterScript>().prefabName = prefabName;
			// if (charactersInScene == null)
			// {
			//     charactersInScene = new Dictionary<string, GameObject>();
			// }
			if (charName != "")
			{
				charactersInScene.Add(charName, characterObject);
				characterObject.GetComponent<CharacterScript>().charName = charName;
			}
			else
			{

				charactersInScene.Add(prefabName, characterObject);
			}
			return characterObject;
		}


		// deletes all characters in the scene
		public void emptyScene()
		{
			// foreach (GameObject character in charactersInScene.Values)
			// {
			//     Debug.Log("Destroying " + character.GetComponent<CharacterScript>().charName);
			//     Destroy(character);
			// }
			foreach (Transform child in characterLayer.transform)
			{
				Destroy(child.gameObject);
			}
			charactersInScene.Clear();

			// foreach (Transform transform in characterLayer.transform)
			// {
			//     GameObject.Destroy(transform.gameObject);
			// }
		}


		public void changeAnimation(string charName, string animName)
		{
			GameObject character;
			charactersInScene.TryGetValue(charName, out character);
			character.GetComponent<CharacterScript>().UseAnimation(animName);
		}

		public void changeExpression(string charName, string exrpName)
		{
			GameObject character;
			charactersInScene.TryGetValue(charName, out character);
			character.GetComponent<CharacterScript>().changeExpression(exrpName);
		}

		// moves a character horizontally to a position.
		public void moveCharacter(string charName, string positionRaw)
		{
			GameObject character;
			charactersInScene.TryGetValue(charName, out character);

			positionRaw = positionRaw.Trim();
			float positionx = float.Parse(positionRaw);

			Vector2 targetPosition = new Vector2(positionx, 0);
			character.GetComponent<CharacterScript>().MoveTo(targetPosition, 5, true);
		}

		public void changeLightingFocus()
		{
			foreach (string key in Instance.charactersInScene.Keys)
			{
				GameObject obj;
				Instance.charactersInScene.TryGetValue(key, out obj);
				CharacterScript script = obj.GetComponent<CharacterScript>();

				if (script.focused)
				{
					if (key.Trim().Equals(DialogueSystem.Instance.CurrentSpeaker.Trim()))
					{
						// script.lightingIntoFocus();
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
						// script.lightingOutOfFocus();
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
}