namespace B1NARY
{
	using System;
	using UnityEngine;
	using Logging;

	/// <summary>
	/// Handles data that are usually called as <see cref="Resources.Load(string)"/>
	/// </summary>
	/// <typeparam name="T"> The <see cref="UnityEngine.GameObject"/> to load from.</typeparam>
	public sealed class ResourcesAsset<T> where T : UnityEngine.Object
	{
		public static implicit operator T(ResourcesAsset<T> voiceLine)
			=> voiceLine.target;

		public readonly T target;
		public ResourcesAsset(string resourcesVAPath, bool throwErrorIfNull = true)
		{
			target = Resources.Load<T>(resourcesVAPath);
			B1NARYConsole.Log(nameof(ResourcesAsset<T>), $"Loading asset: {resourcesVAPath}");
			if (target == null && throwErrorIfNull)
				throw new InvalidOperationException($"Voice Actor line '{resourcesVAPath}' " +
					"does not have an exact filename! Did you leave a space within the filename?");
		}
		public ResourcesAsset(T item) => target = item;
		~ResourcesAsset()
		{
			B1NARYConsole.Log(nameof(ResourcesAsset<T>),
				$"Disposing Asset: {(target == null ? "none" : target.name)}");
			if (target != null)
				Resources.UnloadAsset(target);
		}
	}
}