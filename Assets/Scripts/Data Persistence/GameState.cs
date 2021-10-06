using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class GameState
{
    public Dictionary<string, string> strings;
    public Dictionary<string, bool> bools;
    public Dictionary<string, int> ints;
    public Dictionary<string, float> floats;

    public ArrayList characters;


    // current script file
    public string script;
    // line index of current script
    public int index;
    public string scene;

    [System.Serializable]
    public class CharacterSnapshot
    {
        public string name;
        public string prefabName;
        public float positionX;
        public float positionY;
        public string expression;
        public string animation;
        public bool focused;

        public CharacterSnapshot(string name, string prefabName, string expression, float positionX, float positionY, string animation, bool focused)
        {
            this.name = name;
            this.prefabName = prefabName;
            this.positionX = positionX;
            this.positionY = positionY;
            this.expression = expression;
            this.animation = animation;
            this.focused = focused;
        }
    }

    public GameState()
    {
        strings = new Dictionary<string, string>();
        bools = new Dictionary<string, bool>();
        ints = new Dictionary<string, int>();
        floats = new Dictionary<string, float>();
        characters = new ArrayList();
        // script = "";
        index = 0;
        // scene = "";
    }

    public void captureState()
    {
        characters.Clear();
        scene = SceneManager.GetActiveScene().name;
        script = ScriptParser.Instance.scriptName;
        index = ScriptParser.Instance.currentNode.index;
        Dictionary<string, GameObject> charactersInScene = CharacterManager.Instance.charactersInScene;
        foreach (GameObject characterObject in charactersInScene.Values)
        {
            CharacterScript character = characterObject.GetComponent<CharacterScript>();
            // float positionX = character.isMoving ? character.currentPosition.x : character.targetPosition.x;
            // float positionY = character.isMoving ? character.currentPosition.y : character.targetPosition.y;
            float positionX = character.currentPosition.x;
            float positionY = character.currentPosition.y;

            characters.Add(new CharacterSnapshot(
                character.charName,
                character.prefabName,
                character.currentExpression,
                positionX,
                positionY,
                character.currentAnimation,
                character.focused
            ));
        }
    }

}