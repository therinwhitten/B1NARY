using System;
using UnityEngine;

public abstract class SingletonAlt<T> : InstanceHolder<T> where T : MonoBehaviour
{
	private static T instance;
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

	private void Start()
	{
		if (instance != null)
		{
			Debug.LogError($"{typeof(T)} already have a singleton in the scene! Deleting..");
			Destroy(this);
			return; // Just in case.
		}
		Instance = GetComponent<T>();
		SingletonStart();
	}

	private void OnDestroy()
	{
		instance = null;
		OnSingletonDestroy();
	}




	/// <summary>
	/// Alternate Start since the original start is taken.
	/// </summary>
	protected virtual void SingletonStart() { /* do nothing unless overrided */ }

	/// <summary>
	/// Alternate OnDestroy since the original start is taken.
	/// </summary>
	protected virtual void OnSingletonDestroy() { /* do nothing unless overrided */ }
}