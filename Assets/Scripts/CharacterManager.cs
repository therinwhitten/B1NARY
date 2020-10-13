using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    static string prefabsPath = "Characters/Prefabs/";

    [SerializeField] public GameObject characterLayer;
    // Start is called before the first frame update

    public static CharacterManager instance;

    void Start()
    {
        instance = this;
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
                GameObject character = Instantiate(Resources.Load<GameObject>(prefabsPath + name));
                character.transform.parent = transform;
                character.transform.position = transform.position;
                return;
            }
        }
    }
}