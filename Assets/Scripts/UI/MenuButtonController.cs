using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MenuButtonController : MonoBehaviour
{
    public int x, y;
    [SerializeField] int maxX, maxY;
    bool keyDown;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;
        y = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (!keyDown)
            {
                if (Input.GetAxis("Vertical") < 0)
                {
                    if (y < maxY)
                    {
                        y++;
                    }
                    else
                    {
                        y = 0;
                    }
                }
                else if (Input.GetAxis("Vertical") > 0)
                {
                    if (y > 0)
                    {
                        y--;
                    }
                    else
                    {
                        y = maxY;
                    }
                }
                keyDown = true;
            }
        }
        else
        {
            keyDown = false;
        }
    }

}
