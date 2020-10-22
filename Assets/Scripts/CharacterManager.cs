using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Motion;

public class CharacterManager : MonoBehaviour
{
    static string prefabsPath = "Characters/Prefabs/";
    [SerializeField] public GameObject characterLayer;
    public static CharacterManager instance;
    GameObject testChar;

    Dictionary<string, GameObject> charactersInScene;

    void Start()
    {
        instance = this;
        charactersInScene = new Dictionary<string, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void spawnCharacter(string name)
    {
        name = name.Trim();
        foreach (Transform transform in characterLayer.transform)
        {
            if (transform.childCount == 0)
            {
                GameObject characterObject = Instantiate(Resources.Load<GameObject>(prefabsPath + name));
                characterObject.transform.parent = transform;
                characterObject.transform.position = transform.position;
                charactersInScene.Add(name, characterObject);
                return;
            }
        }
    }

    public void emptyScene()
    {
        charactersInScene = new Dictionary<string, GameObject>();
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
}