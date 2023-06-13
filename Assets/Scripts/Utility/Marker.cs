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
		/// Finds all gameobjects with the <paramref name="targetName"/> and with 
		/// <see cref="Marker"/> in the current scene. This ignores any 
		/// enable-disable stuffs.
		/// </summary>
		/// <param name="targetName"> The name to get. </param>
		/// <returns> 
		/// An enumerable, due to possibly finding multiple. use something like
		/// <see cref="Enumerable.First{TSource}(IEnumerable{TSource})"/> for a
		/// single object.
		/// </returns>
		public static IEnumerable<Marker> GetMarkers(string targetName)
		{
			// https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html
			Marker[] markers = Resources.FindObjectsOfTypeAll<Marker>();
			for (int i = 0; i < markers.Length; i++)
			{
				if (markers[i].Name != targetName)
					continue;
				if (markers[i] == null)
					continue;
				if (markers[i].hideFlags != HideFlags.None)
					continue;
				yield return markers[i];
			}
		}
		/// <summary>
		/// Finds all gameobjects with the <paramref name="targetName"/> and with 
		/// <see cref="Marker"/> in the current scene. This ignores any 
		/// enable-disable stuffs.
		/// </summary>
		/// <param name="targetName"> The name to get. </param>
		/// <returns> 
		/// An enumerable, due to possibly finding multiple. use something like
		/// <see cref="Enumerable.First{TSource}(IEnumerable{TSource})"/> for a
		/// single object.
		/// </returns>
		public static IEnumerable<GameObject> GetInactiveGameobjects(string targetName)
		{
			return GetMarkers(targetName).Select(marker => marker.gameObject);
		}
		public string Name => name;
	}
}