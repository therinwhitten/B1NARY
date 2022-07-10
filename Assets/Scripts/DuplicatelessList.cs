using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DuplicatelessList<T> : IEnumerable<T>
{
	private HashSet<T> containingValues = new HashSet<T>();
	private List<T> data = new List<T>();
	public void Add(T value)
	{
		if (containingValues.Contains(value))
			throw new InvalidOperationException();
		data.Add(value);
		containingValues.Add(value);
	}
	public void Remove(T value)
	{
		if (!containingValues.Contains(value))
			return;
		data.Remove(value);
		containingValues.Remove(value);
	}
	public void RemoveAt(int index)
	{
		T value = data[index];
		data.RemoveAt(index);
		containingValues.Remove(value);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)data).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)data).GetEnumerator();
	}
}