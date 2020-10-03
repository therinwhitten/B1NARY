using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotesSystem : MonoBehaviour
{
    public static EmotesSystem instance;

    GameObject character;
    public Image characterSprite;

    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.Find("Character[Saki]");
    }

    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void changeEmote(string emoteName)
    {
        switch (emoteName)
        {
            case "angry":
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_angry");
                break;
            case "curious":
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_curious");
                break;
            case "happy":
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_happy");
                break;
            case "surprised":
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_surprised");
                break;
            case "worried":
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_worried");
                break;
            default:
                characterSprite.sprite = Resources.Load<Sprite>("img/Characters/saki/saki_normal");
                break;

        }
    }
}
