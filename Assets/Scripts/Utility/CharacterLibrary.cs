using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLibrary : MonoBehaviour
{
    public static CharacterLibrary instance;
    public Dictionary<string, CharacterEntry> library;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        library = new Dictionary<string, CharacterEntry>();
        CharacterEntry entry = new CharacterEntry
        {
            // HARU
            expressions = new string[] {
                "Angry",
                "Blushing",
                "Normal",
                "Sad",
                "Smile",
                "Surprised",
                "f01",
                "f02" }
        };
        library.Add("Haru", entry);


        // EPSILON
        entry = new CharacterEntry
        {
            expressions = new string[] {
                "Angry",
                "Blushing",
                "Normal",
                "Sad",
                "Smile",
                "Surprised",
                "f01",
                "f02" }
        };
        library.Add("Epsilon", entry);
    }

    public CharacterEntry getEntry(string key)
    {
        CharacterEntry entry;
        library.TryGetValue(key, out entry);
        if (entry == null)
        {
            library.TryGetValue("Haru", out entry);
        }
        return entry;
    }


}
