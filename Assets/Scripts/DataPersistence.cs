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
    }
    private void Start()
    {
        initialize();
    }

}