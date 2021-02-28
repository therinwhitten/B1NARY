using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentData : Singleton<PersistentData>
{
    // Start is called before the first frame update
    void Awake()
    {
        initialize();
    }
    public override void initialize()
    {
    }
    // Update is called once per frame
    void Update()
    {

    }
}
