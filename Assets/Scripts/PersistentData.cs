using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PersistentData : Singleton<PersistentData>
{
    string path;
    public GameState state;
    // Start is called before the first frame update
    void Awake()
    {
        initialize();
    }
    public override void initialize()
    {
        path = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string[] filenames = Directory.GetFiles(path);
        // if (filenames.Length == 0)
        state = new GameState();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
