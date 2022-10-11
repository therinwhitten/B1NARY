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
	public class Multiton<T> : InstanceHolder<T> where T : MonoBehaviour
	{
		/// <summary>
		/// Checks if the current instances of <see cref="Multiton{T}"/> has 
		/// their <see cref="Awake"/> or <see cref="OnDestroy"/> private methods
		/// overridden, and screams if they do.
		/// </summary>
		[ExecuteInEditMode]
		private static void MultitonInitializer()
		{
			Type baseAwakeType = typeof(Multiton<T>).GetMethod(nameof(Awake)).DeclaringType,
				baseOnDestroyType = typeof(Multiton<T>).GetMethod(nameof(OnDestroy)).DeclaringType;
			(from type in typeof(Multiton<T>).Assembly.GetTypes()
			 where type == typeof(Multiton<T>)
			 let awakeInfo = type.GetMethod(nameof(Awake)).DeclaringType
			 let onDestroyInfo = type.GetMethod(nameof(OnDestroy)).DeclaringType
			 where awakeInfo != baseAwakeType || onDestroyInfo != baseOnDestroyType
			 select type).Select(type =>
			 {
				 Debug.LogError($"{type.Name} is found" +
				 $" to have overridden {nameof(Awake)} or {nameof(OnDestroy)}, please fix!");
				 return type;
			 });
		}
		/// <summary> The data and their assigned keys. </summary>
		private static Dictionary<int, T> instances = new Dictionary<int, T>();
		/// <summary>
		/// The keys of <see cref="instances"/>. Can be used as a list using
		/// <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/>
		/// </summary>
		private static IEnumerable<int> instancesKeys => instances.Keys;
		private static int nextIndex = 0;
		public static T First => instances[instances.Keys.Min()];
		public static T Last => instances[instances.Keys.Max()];
		public static bool Any => instances.Any();
		public static T GetViaIndex(int index) => instances.ElementAt(index).Value;
		public static T GetViaID(int ID) => instances[ID];
		public static IEnumerator<T> GetEnumerator()
		{
			foreach (T item in instances.Values)
				yield return item;
		}
		public static IEnumerable<T> AsEnumerable()
		{
			foreach (T item in instances.Values)
				yield return item;
		}

		public int Index { get; private set; }
		private void Awake()
		{
			Index = nextIndex;
			nextIndex++;
			instances.Add(Index, GetComponent<T>());
			MultitonAwake();
		}
		private void OnDestroy()
		{
			instances.Remove(Index);
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