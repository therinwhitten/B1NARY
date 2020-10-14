using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Expression;

public class Character
{
    public string charName;
    public CharacterLibrary library;
    public CharacterEntry libraryEntry;
    protected string[] expressions;
    public GameObject gameObject;
    public Character(string name)
    {
        library = CharacterLibrary.instance;

        this.charName = name;
        libraryEntry = library.getEntry(charName);

        expressions = libraryEntry.expressions;

        // gameObject.GetComponent<CubismExpressionController>().changeExpre;
    }

    public void animate(string animName)
    {
        gameObject.GetComponent<Animator>().SetTrigger(animName.Trim());
    }

    public void changeExpression(string expressionName)
    {
        int expressionIndex = System.Array.IndexOf(expressions, expressionName);
        // Debug.Log("Changing expression of character " + charName + ". Name: " + expressionName + ". Index: " + expressionIndex);
        gameObject.GetComponent<CubismExpressionController>().CurrentExpressionIndex = expressionIndex;
    }

}