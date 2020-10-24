﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void spawnCharacter(string name, string positionRaw = "0.5")
    {
        Transform transform = characterLayer.transform;

        GameObject characterObject = Instantiate(Resources.Load<GameObject>(prefabsPath + name), transform);
        characterObject.transform.position = new Vector3(transform.position.x, characterObject.transform.position.y, characterObject.transform.position.z);
        characterObject.GetComponent<CharacterScript>().SetPosition(new Vector2(float.Parse(positionRaw), 0));
        charactersInScene.Add(name, characterObject);
        return;

    }

    // deletes all characters in the scene
    public void emptyScene()
    {
        charactersInScene = new Dictionary<string, GameObject>();
        foreach (Transform transform in characterLayer.transform)
        {
            GameObject.Destroy(transform.gameObject);
        }
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
}