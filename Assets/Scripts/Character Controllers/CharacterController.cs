namespace B1NARY
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;

	public sealed class CharacterController : SingletonAlt<CharacterController>
	{
		public const string prefabsPath = "Characters/Prefabs/";
		public static readonly IReadOnlyDictionary<string, Delegate> CharacterDelegateCommands = new Dictionary<string, Delegate>()
		{
			["spawnchar"] = (Action<string, string, string>)((prefabName, positionRaw, characterName) =>
			{
				Instance.SummonCharacter(prefabName, positionRaw, characterName);
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
			charactersInScene.Add(newKey, pair);
		}


		protected override void SingletonAwake()
		{
			DontDestroyOnLoad(characterLayer);
			charLayerTransform = characterLayer.transform;
		}

		public KeyValuePair<string, (GameObject gameObject, CharacterScript characterScript)> 
			SummonCharacter(string prefabName, string positionRaw, string charName = "")
		{
			GameObject output = Resources.Load<GameObject>(prefabsPath + prefabName);
			if (output == null)
				throw new FileNotFoundException($"character prefab '{prefabName}'" +
					$" is not found in {prefabsPath}{prefabName}!", 
					Application.streamingAssetsPath + prefabsPath + prefabName);
			output = Instantiate(output, charLayerTransform);
			string outputName;
			if (output.TryGetComponent(out CharacterScript characterScript))
			{
				if (string.IsNullOrEmpty(charName) == false)
					characterScript.PrefabName = charName;
				outputName = characterScript.PrefabName;
			}
			else
			{
				if (string.IsNullOrEmpty(charName) == false)
					output.name = charName;
				outputName = output.name;
				Debug.LogError($"{output.name} does not have a {nameof(CharacterScript)} component!", this);
			}
			output.SetActive(true);
			charactersInScene.Add(outputName, (output, characterScript));
			return new KeyValuePair<string, (GameObject gameObject, CharacterScript characterScript)>
				(outputName, (output, characterScript));
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