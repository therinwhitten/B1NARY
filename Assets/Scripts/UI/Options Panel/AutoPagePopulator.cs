namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public class AutoPagePopulator : MonoBehaviour
	{
		public GameObject row;


		public int objectsPerRow = 3;

		private int internalCounter = 0;
		private GameObject CurrentRow => rows[rows.Count - 1];
		private List<GameObject> rows;

		protected virtual void Awake()
		{
			internalCounter = objectsPerRow;
			rows = new List<GameObject>();
		}
		public void Clear()
		{
			internalCounter = objectsPerRow;
			rows.ForEach(row => Destroy(row));
			rows.Clear();
		}
		public virtual GameObject AddEntry(GameObject copy)
		{
			if (internalCounter >= objectsPerRow)
			{
				internalCounter = 0;
				rows.Add(gameObject.AddChildObject(row));
			}
			internalCounter++;
			return CurrentRow.AddChildObject(copy);
		}
	}

	
}
