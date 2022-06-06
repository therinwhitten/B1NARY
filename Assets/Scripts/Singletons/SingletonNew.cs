using System;
using UnityEngine;

///<summary> 
/// A class that stores a single instance of <see cref="T"/>,
/// created when called for the first time
/// </summary>
public class SingletonNew<T> : MonoBehaviour where T : MonoBehaviour
{
	/// <summary> 
	///		If there is no instances created in unity, throw an error instead
	///		of creating a custom GameObject for it.
	/// </summary>
	public static bool ThrowErrorIfEmpty { get; set; } = true;
	private static object _lock = new object();

	private static T _instance;
	/// <summary> First Registered Instance in the scene </summary>
	public static T Instance
	{
		get
		{
			if (_instance != null)
				return _instance;
			lock (_lock)
			{
				if (_instance != null)
					return _instance;
				if (ThrowErrorIfEmpty)
					throw new ArgumentNullException($"{typeof(T)} does not " +
						"have an instance created!");
				var @object = new GameObject($"{typeof(T)} (Singleton)");
				_instance = @object.AddComponent<T>();
				return _instance;
			}
		}
		set => _instance = value;
	}

	private bool isLinkedInstance = false;
	private void Start()
	{
		if (_instance == null)
		{
			isLinkedInstance = true;
			Instance = GetComponent<T>();
		}
		SingletonStart();
	}

	private void OnDestroy()
	{
		if (isLinkedInstance)
			_instance = null;
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