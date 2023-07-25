namespace B1NARY.UI
{
	using System.Collections.Generic;
	using UnityEngine;

	public class AutoPagePopulator : MonoBehaviour
	{
		public GameObject row;


		public int objectsPerRow = 3;

		private int internalCounter = 0;

		private List<(GameObject row, List<GameObject> columns)> m_rows;

		protected virtual void Awake()
		{
			internalCounter = objectsPerRow;
			m_rows = new List<(GameObject row, List<GameObject> columns)>();
		}
		public virtual void Clear()
		{
			internalCounter = objectsPerRow;
			m_rows.ForEach(row => Destroy(row.row));
			m_rows.Clear();
		}
		public virtual GameObject AddEntry(GameObject copy)
		{
			if (internalCounter >= objectsPerRow)
			{
				internalCounter = 0;
				m_rows.Add((gameObject.InstantiateChildObject(row), new List<GameObject>()));
			}
			internalCounter++;
			GameObject slot = m_rows[^1].row.InstantiateChildObject(copy);
			m_rows[^1].columns.Add(slot);
			return slot;
		}
	}
}
