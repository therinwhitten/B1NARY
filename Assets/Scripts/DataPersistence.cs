using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public class DataPersistence : Singleton<DataPersistence>
{
    string path;
    public override void initialize()
    {
        path = Application.persistentDataPath;
        Debug.Log(path);
    }
    private void Start()
    {
        initialize();
    }

}