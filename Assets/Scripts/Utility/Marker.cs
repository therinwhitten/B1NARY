namespace B1NARY
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// A simple component that is used for things such as 
	/// <see cref="GameObject.Find(string)"/> for disabled gameobjects.
	/// </summary>
	public sealed class Marker : MonoBehaviour
	{
		/// <summary>
		/// Finds all gameobjects with the <paramref name="name"/> and with 
		/// <see cref="Marker"/> in the current scene. This ignores any 
		/// enable-disable stuffs.
		/// </summary>
		/// <param name="name"> The name to get. </param>
		/// <returns> 
		/// An enumerable, due to possibly finding multiple. use something like
		/// <see cref="Enumerable.First{TSource}(IEnumerable{TSource})"/> for a
		/// single object.
		/// </returns>
		public static IEnumerable<GameObject> FindWithMarker(string name)
		{
			// https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html
#pragma warning disable CS0618 
			// Type or member is obsolete
			// The system that the source recommends doesn't work properly, even
			// - even when copied. So just using an obselete method will have to 
			// - do.
			Marker[] markers = FindObjectsOfTypeAll(typeof(Marker)) as Marker[];
			Debug.Log(string.Join($",\n", markers.Select(marker => marker.name)));
			for (int i = 0; i < markers.Length; i++)
			{
				//if (!markers[i].SameName(name))
				//	continue;
				//if (!markers[i].InScene())
				//	continue;
				yield return markers[i].gameObject;
			}
		}
		///// <summary>
		///// Finds all gameobjects with the <paramref name="name"/>. Unlike
		///// <see cref="GameObject.Find(string)"/>, This ignores any enable-disable 
		///// stuff.
		///// </summary>
		///// <param name="name"> The name to get. </param>
		///// <returns> 
		///// An enumerable, due to possibly finding multiple. use something like
		///// <see cref="Enumerable.First{TSource}(IEnumerable{TSource})"/> for a
		///// single object.
		///// </returns>
		//public static IEnumerable<GameObject> Find(string name)
		//{
		//	// https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html
		//	using (IEnumerator<GameObject> arr = (Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]).AsEnumerable().GetEnumerator())
		//		while (arr.MoveNext())
		//		{
		//			GameObject currentObject = arr.Current;
		//			if (currentObject.name != name)
		//				continue;
		//			if (!(currentObject.hideFlags == HideFlags.NotEditable || currentObject.hideFlags == HideFlags.HideAndDontSave))
		//				continue;
		//			yield return currentObject;
		//		}
		//}
		//
		//public bool SameName(string otherName)
		//{
		//	return gameObject.name == otherName;
		//}
		//public bool InScene()
		//{
		//	return gameObject.hideFlags == HideFlags.NotEditable || gameObject.hideFlags == HideFlags.HideAndDontSave;
		//}
	}
}