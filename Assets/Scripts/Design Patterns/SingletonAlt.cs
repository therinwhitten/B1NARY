﻿using System;
using UnityEngine;


/// <summary>
///		Limits a single object in the world, and allows use of <see cref="Instance"/>
///		instead of using fields for monoBehaviours.
/// </summary>
/// <typeparam name="T">A MonoBehaviour Script to tie it to.</typeparam>
public abstract class SingletonAlt<T> : InstanceHolder<T> where T : MonoBehaviour
{
	private static T instance;
	/// <summary> A Single instance </summary>
	public static T Instance
	{
		get
		{
			if (instance != null)
				return instance;
			lock (_lock)
			{
				if (instance != null)
					return Instance;
				T instFind = FindObjectOfType<T>();
				if (instFind != null)
				{
					instance = instFind;
					return instFind;
				}
				if (ThrowErrorIfEmpty)
					throw new MissingComponentException($"{typeof(T)} does not " +
						"have an instance created!");
				var @object = new GameObject($"{typeof(T)} (Singleton)");
				instance = @object.AddComponent<T>();
				return instance;
			}
		}
		set => instance = value;
	}

	/// <summary>
	///		Override <see cref="Instance"/> with a new Instance.
	/// </summary>
	/// <param name="object"> The object you want to have the component in. </param>
	public static void OverrideWithNew(GameObject @object)
	{
		instance = null;
		Instance = @object.AddComponent<T>();
	}

	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogError($"{typeof(T)} already has a singleton in the scene! Deleting..");
			Destroy(this);
			return; // Just in case.
		}
		Instance = GetComponent<T>();
		SingletonAwake();
	}

	private void OnDestroy()
	{
		instance = null;
		OnSingletonDestroy();
	}




	/// <summary>
	/// Alternate Start since the original start is taken.
	/// </summary>
	protected virtual void SingletonAwake() { /* do nothing unless overrided */ }

	/// <summary>
	/// Alternate OnDestroy since the original start is taken.
	/// </summary>
	protected virtual void OnSingletonDestroy() { /* do nothing unless overrided */ }
}
public abstract class InstanceHolder<T> : MonoBehaviour where T : MonoBehaviour
{
	/// <summary> 
	///		If there is no instances created in unity, throw an error instead
	///		of creating a custom GameObject for it.
	/// </summary>
	public static bool ThrowErrorIfEmpty { get; set; } = true;
	protected static object _lock = new object();
}