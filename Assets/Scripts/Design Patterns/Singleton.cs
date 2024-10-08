﻿namespace B1NARY.DesignPatterns
{
	using System.Linq;
	using UnityEngine;


	/// <summary>
	///		Limits a single object in the world, and allows use of <see cref="Instance"/>
	///		instead of using fields for monoBehaviours.
	/// </summary>
	/// <typeparam name="T">A MonoBehaviour Script to tie it to.</typeparam>
	public abstract class Singleton<T> : InstanceHolder<T> where T : MonoBehaviour
	{
		public static T ForceFind()
		{
			T toughFind = Resources.FindObjectsOfTypeAll(typeof(T)).FirstOrDefault() as T;
			if (toughFind == null)
				throw new System.Exception("Sexy");
			return toughFind;
		}
		public static bool HasInstance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<T>();
				return instance != null;
			}
		}
		public static bool HasInstanceOrInitialize()
		{
			bool output = HasInstance;
			if (output == false && !ThrowErrorIfEmpty)
			{
				_ = InstanceOrDefault;
				return true;
			}
			return output;
		}

		protected static T instance;

		/// <summary> A Single instance. </summary>
		public static T InstanceOrDefault
		{
			get
			{
				bool oldValue = ThrowErrorIfEmpty;
				ThrowErrorIfEmpty = false;
				T instance = Instance;
				ThrowErrorIfEmpty = oldValue;
				return instance;
			}
		}
		/// <summary> A Single instance. </summary>
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
					DontDestroyOnLoad(@object);
					instance = @object.AddComponent<T>();
					return instance;
				}
			}
			set => instance = value;
		}

		public static bool TryGetInstance(out T instance)
		{
			if (!HasInstance)
			{
				instance = null;
				return false;
			}
			instance = Instance;
			return true;
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

		protected void Awake()
		{
			if (instance != null && instance != this)
			{
				Debug.LogError($"{typeof(T)} already has a singleton in the scene! " +
					$"Deleting..\nAll Objects: {string.Join("\n\t", FindObjectsOfType<T>().Select(data => $"{data.name}"))}");
				Destroy(this);
				return; // Just in case.
			}
			Instance = GetComponent<T>();
			SingletonAwake();
		}

		protected void OnDestroy()
		{
			instance = null;
			OnSingletonDestroy();
		}




		/// <summary>
		/// Alternate Awake since the original start is taken.
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
		protected static object _lock = new();
	}
}