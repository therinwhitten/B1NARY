namespace B1NARY.DesignPatterns
{
	using System;
	using System.Reflection;
	using System.Linq;
	using UnityEngine;
	using System.Collections.Generic;

	///<summary> 
	/// A list of multiple <see cref="MonoBehaviour"/>s, usage as such.
	/// </summary>
	public class Multiton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static readonly LinkedList<T> instances = new();
		public static T[] GetNewInstancesList() => instances.ToArray();
		public static T Oldest => instances.First.Value;
		public static T Youngest => instances.Last.Value;
		public static bool Any => instances.Count > 0;
		public static int Count => instances.Count;

		private LinkedListNode<T> ID;

		protected void Awake()
		{
			ID = instances.AddLast(this as T);
			MultitonAwake();
		}
		protected void OnDestroy()
		{
			instances.Remove(ID);
			OnMultitonDestroy();
		}


		/// <summary>
		/// Alternate Awake since the original start is taken.
		/// </summary>
		protected virtual void MultitonAwake() { /* do nothing unless overrided */ }
		/// <summary>
		/// Alternate OnDestroy since the original start is taken.
		/// </summary>
		protected virtual void OnMultitonDestroy() { /* do nothing unless overrided */ }
	}
}