namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;

	public class CachedMonobehaviour : MonoBehaviour
	{
		/// <summary>
		/// Clones a component by copying its internal field values in the class
		/// and setting it to the new Component.
		/// </summary>
		/// <param name="target"> The component to copy. </param>
		/// <returns> The cloned <typeparamref name="TComponent"/>. </returns>
		public static TComponent Clone<TComponent>(TComponent target) where TComponent : Component
		{
			TComponent output = target.gameObject.AddComponent<TComponent>();
			FieldInfo[] fields = typeof(TComponent)
				.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			for (int i = 0; i < fields.Length; i++)
				fields[i].SetValue(output, fields[i].GetValue(target));
			return output;
		}



		private Dictionary<string, Component> m_cachedComponents;
		private Dictionary<string, Component> CachedComponents
		{
			get
			{
				m_cachedComponents ??= new Dictionary<string, Component>();
				return m_cachedComponents;
			}
		}
		public TComponent GetComponent<TComponent>(bool includeChildObjects = false, 
			bool includeParentObjects = false) where TComponent : Component
		{
			string name = typeof(TComponent).Name;
			if (CachedComponents.TryGetValue(name, out Component comp))
				if (comp != null)
					return (TComponent)comp;
				else
					CachedComponents.Remove(name);
			if (base.TryGetComponent<TComponent>(out var component))
				return (TComponent)(CachedComponents[name] = component);
			if (includeChildObjects)
			{
				component = gameObject.GetComponentInChildren<TComponent>();
				if (component != null)
					return (TComponent)(CachedComponents[name] = component);
			}
			if (includeParentObjects)
			{
				component = gameObject.GetComponentInParent<TComponent>();
				if (component != null)
					return (TComponent)(CachedComponents[name] = component);
			}
			return null;
		}

		/// <summary>
		/// Adds onto <see cref="GetComponent{TComponent}(GameObject, bool, bool)"/>,
		/// but adds the <paramref name="gameObject"/> if there are none found.
		/// </summary>
		/// <typeparam name="TComponent"> A component to get or add. </typeparam>
		/// <param name="gameObject"> 
		/// Where to get the components from. Also the gameobject to add to. 
		/// </param>
		/// <param name="includeChildObjects"> 
		/// If child objects from 1-depth search are allowed. 
		/// </param>
		/// <param name="includeParentObjects"> 
		/// If parent objects from 1-depth search are allowed.
		/// </param>
		/// <returns> 
		/// The <typeparamref name="TComponent"/>.
		/// </returns>
		public TComponent GetOrAddComponent<TComponent>(bool includeChildObjects = false, 
			bool includeParentObjects = false) where TComponent : Component
		{
			TComponent output = GetComponent<TComponent>(includeChildObjects, includeParentObjects);
			if (output == null)
				return (TComponent)(CachedComponents[name] = gameObject.AddComponent<TComponent>());
			return output;
		}

		/// <summary>
		/// Checks if a gameobject has the component to get.
		/// </summary>
		/// <typeparam name="TComponent"> A component to get. </typeparam>
		/// <param name="gameObject"> Where to get the components from. </param>
		/// <param name="includeChildObjects"> 
		/// If child objects from 1-depth search are allowed. 
		/// </param>
		/// <param name="includeParentObjects"> 
		/// If parent objects from 1-depth search are allowed.
		/// </param>
		public bool ContainsComponent<TComponent>(bool includeChildObjects = false, 
			bool includeParentObjects = false) where TComponent : Component
		{
			return GetComponent<TComponent>(includeChildObjects, includeParentObjects) != null;
		}

		public bool TryGetComponent<TComponent>(out TComponent component, 
			bool includeChildObjects = false,
			bool includeParentObjects = false) where TComponent : Component
		{
			component = GetComponent<TComponent>(includeChildObjects, includeParentObjects);
			return component != null;
		}
	}
}
