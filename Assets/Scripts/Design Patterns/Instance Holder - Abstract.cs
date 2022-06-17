using UnityEngine;

public abstract class InstanceHolder<T> : MonoBehaviour where T : MonoBehaviour
{
	/// <summary> 
	///		If there is no instances created in unity, throw an error instead
	///		of creating a custom GameObject for it.
	/// </summary>
	public static bool ThrowErrorIfEmpty { get; set; } = true;
	protected static object _lock = new object();
}