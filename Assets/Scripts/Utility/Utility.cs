﻿namespace B1NARY
{
	using System;
	using UnityEngine;

	public static class Utility
	{
		public static GameObject AddChildObject(this GameObject parent, GameObject copy)
		{
			var obj = UnityEngine.Object.Instantiate(copy, parent.transform);
			return obj;
		}
	}
}