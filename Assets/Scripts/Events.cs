using System;
using UnityEngine;

public delegate void UnityEventHandler(MonoBehaviour sender, EventArgs args);
public delegate void UnityEventHandler<T>(MonoBehaviour sender, T args);