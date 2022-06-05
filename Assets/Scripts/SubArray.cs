using System;
using System.Linq;
using System.Collections.Generic;

// TODO: Add list interfaces!
public class SubArray<T> 
{
	private Ref<T[]> referencedArray;
	private int startIndex = 0, endIndex = int.MaxValue;
	public int StartIndex 
	{ 
		get => startIndex;
		set 
		{
			if (value > referencedArray.Value.Length)
				throw new IndexOutOfRangeException($"{nameof(value)} {startIndex}"
					+ $" is larger than the referenced array of {referencedArray.Value.Length}");
			else if (value < 0)
				throw new ArgumentException($"value cannot be below 0!", nameof(value));
			startIndex = value;
		} 
	}
	public int EndIndex
	{
		get => endIndex;
		set
		{
			if (EndIndex < StartIndex)
				throw new ArgumentException("value cannot be below " +
					$"{nameof(StartIndex)} with {StartIndex}!", nameof(value));
			endIndex = value;
		}
	}
	public int Length => 
		new int[] { referencedArray.Value.Length, endIndex }.Min() - startIndex;

	public SubArray(Ref<T[]> referencedArray, int startIndex)
	{
		this.referencedArray = referencedArray;
		StartIndex = startIndex;
	}
	public SubArray(Ref<T[]> referencedArray, int startIndex, int endIndex)
	{
		this.referencedArray = referencedArray;
		StartIndex = startIndex;
		EndIndex = endIndex;
	}

	public T this[int index]
	{
		get
		{
			index += startIndex;
			CheckRange(index);
			return referencedArray.Value[index];
		}
		set
		{
			index += startIndex;
			CheckRange(index);
			referencedArray.Value[index] = value;
		}
	}
	private void CheckRange(int index)
	{

		if (index > endIndex)
			throw new IndexOutOfRangeException($"{nameof(index)} {index}"
				+ $"with starting Index of {startIndex} is higher than {endIndex}!");
	}

	public T[] ToArray() 
		=> referencedArray.Value.Skip(startIndex).Take(EndIndex).ToArray();
	public List<T> ToList() => ToArray().ToList();
	public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector)
		=> ToArray().ToDictionary(keySelector);

}