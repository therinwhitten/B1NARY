using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Expression;

public class CharacterScript : MonoBehaviour
{
    public string charName;
    public CharacterLibrary library;
    public CharacterEntry libraryEntry;
    public string[] expressions;

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