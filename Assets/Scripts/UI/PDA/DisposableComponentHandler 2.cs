namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public static class DisposableComponentHandler
	{
		public static T Create<T>(T copyComponent) where T : Component
		{
			GameObject disposable = new($"disposable object ({copyComponent.GetType().Name})", typeof(T));
			return copyComponent;
		}
	}
}
