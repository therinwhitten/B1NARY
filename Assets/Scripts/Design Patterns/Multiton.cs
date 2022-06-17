using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

///<summary> 
/// A class that stores a single instance of <see cref="T"/>,
/// created when called for the first time
/// </summary>
public abstract class Multiton<T> : InstanceHolder<T> where T : MonoBehaviour
{
	private static Dictionary<int, T> instances = new Dictionary<int, T>();
	private static Queue<int> queue = new Queue<int>();
	private static List<int> instancesList = new List<int>();
	public static T First()
	{
		CheckInstances();
		return instances[instancesList.Min()];
	}
	public static T Last()
	{
		CheckInstances();
		return instances[instancesList.Max()];
	}
	public static T GetItem(int index)
	{
		CheckInstances();
		return instances[index];
	}

	private static void CheckInstances()
	{
		if (instances.Count == 0)
			lock (_lock)
			{
				if (instances.Count > 0)
					return;
				if (ThrowErrorIfEmpty)
					throw new ArgumentNullException($"{typeof(T)} does not " +
						"have an instance created!");
				var @object = new GameObject($"{typeof(T)} (Singleton)");
				instances.Add(instances.Count, @object.AddComponent<T>());
			}
	}


	private int index;
	private void Start()
	{
		index = queue.Count == 0 ? instances.Count : queue.Dequeue();
		instancesList.Add(index);
		instances.Add(index, GetComponent<T>());
		SingletonStart();
	}

	private void OnDestroy()
	{
		queue.Enqueue(index);
		instances.Remove(index);
		instancesList.Remove(index);
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