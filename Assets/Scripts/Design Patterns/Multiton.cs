using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

///<summary> 
/// A list of multiple <see cref="MonoBehaviour"/>s, usage as such.
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

	/// <summary> Index for usage of <see cref="Multiton{T}"/></summary>
	public int MultitonIndex { get; private set; }
	private void Start()
	{
		MultitonIndex = queue.Count == 0 ? instances.Count : queue.Dequeue();
		instancesList.Add(MultitonIndex);
		instances.Add(MultitonIndex, GetComponent<T>());
		SingletonStart();
	}

	private void OnDestroy()
	{
		queue.Enqueue(MultitonIndex);
		instances.Remove(MultitonIndex);
		instancesList.Remove(MultitonIndex);
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