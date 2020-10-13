using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLibrary : MonoBehaviour
{
    public static CharacterLibrary instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }


}
