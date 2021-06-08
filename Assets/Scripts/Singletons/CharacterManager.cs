using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Singleton<CharacterManager>
{
    static string prefabsPath = "Characters/Prefabs/";
    [SerializeField] public GameObject characterLayer;

    public Dictionary<string, GameObject> charactersInScene;

    void Awake()
    {
        initialize();
    }
    public override void initialize()
    {
        charactersInScene = new Dictionary<string, GameObject>();
        characterLayer = GameObject.Find("CharacterLayer");
        // if (characterLayer.transform.childCount != 0)
        // {
        //     foreach (Transform transform in characterLayer.transform)
        //     {
        //         charactersInScene.Add(transform.gameObject.name, transform.gameObject);
        //     }
        // }
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void spawnCharacter(string prefabName, string positionRaw, string charName = "")
    {
        Transform transform = characterLayer.transform;

        GameObject characterObject = Instantiate(Resources.Load<GameObject>(prefabsPath + prefabName), transform);
        characterObject.SetActive(true);
        characterObject.transform.position = new Vector3(transform.position.x, characterObject.transform.position.y, characterObject.transform.position.z);
        characterObject.GetComponent<CharacterScript>().SetPosition(new Vector2(float.Parse(positionRaw), 0));
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
        return;
    }


    // deletes all characters in the scene
    public void emptyScene()
    {
        foreach (GameObject character in charactersInScene.Values)
        {
            Debug.Log("Destroying " + character.GetComponent<CharacterScript>().charName);
            Destroy(character);
        }
        charactersInScene = new Dictionary<string, GameObject>();

        // foreach (Transform transform in characterLayer.transform)
        // {
        //     GameObject.Destroy(transform.gameObject);
        // }
    }


    public void changeAnimation(string charName, string animName)
    {
        GameObject character;
        charactersInScene.TryGetValue(charName, out character);
        character.GetComponent<CharacterScript>().animate(animName);
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
                if (key.Trim().Equals(DialogueSystem.Instance.currentSpeaker.Trim()))
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
                if (key.Trim().Equals(DialogueSystem.Instance.currentSpeaker.Trim()))
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


