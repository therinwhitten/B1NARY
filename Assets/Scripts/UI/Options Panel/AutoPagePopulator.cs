namespace B1NARY.UI
{
	using System;
	using UnityEngine;

	public class AutoPagePopulator : MonoBehaviour
	{
		public GameObject slot;
		public GameObject row;
		public int objectsPerRow = 3;

		private int internalCounter = 0;
		private GameObject currentRow;

		protected virtual void Awake()
		{
			internalCounter = objectsPerRow;
		}
		public virtual GameObject AddEntry()
		{
			if (internalCounter >= objectsPerRow)
			{
				internalCounter = 0;
				currentRow = gameObject.AddChildObject(row);
			}
			internalCounter++;
			return currentRow.AddChildObject(slot);
		}
	}
}
