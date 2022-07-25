using System;
using UnityEngine;

public sealed class ResourcesAsset<T> where T : UnityEngine.Object
{
	public static implicit operator T(ResourcesAsset<T> voiceLine) 
		=> voiceLine.target;

	public readonly T target;
	public ResourcesAsset(string resourcesVAPath, bool throwErrorIfNull = true)
	{
		target = Resources.Load<T>(resourcesVAPath);
		if (target == null && throwErrorIfNull)
			throw new InvalidOperationException($"Voice Actor line '{resourcesVAPath}' " +
				"does not have an exact filename! Did you leave a space within the filename?");

	}
	public ResourcesAsset(T item) => target = item;
	~ResourcesAsset()
	{
		if (target != null)
			Resources.UnloadAsset(target);
	}
}
