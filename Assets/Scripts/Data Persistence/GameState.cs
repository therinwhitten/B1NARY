using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public Dictionary<string, string> strings;
    public Dictionary<string, bool> bools;
    public Dictionary<string, int> ints;
    public Dictionary<string, float> floats;

    public GameState()
    {
        strings = new Dictionary<string, string>();
        bools = new Dictionary<string, bool>();
        ints = new Dictionary<string, int>();
        floats = new Dictionary<string, float>();
    }

}