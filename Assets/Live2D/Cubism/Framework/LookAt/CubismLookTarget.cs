﻿using Live2D.Cubism.Framework.LookAt;
using UnityEngine;

public class CubismLookTarget : MonoBehaviour, ICubismLookTarget
{
    public Vector3 GetPosition()
    {
        if(!Input.GetMouseButton(0))
        {
            return Vector3.zero;
        }

        var targetPosition = Input.mousePosition;

        targetPosition = (Camera.main.ScreenToViewportPoint(targetPosition) * 2) - Vector3.one;

        return targetPosition;
    }


    public bool IsActive()
    {
        return true;
    }
}