using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// <para>
/// Creates and holds a pointer to an array with a start index and a length to
/// limit itself in.
/// </para>
/// </summary>
/// <typeparam name="TValue"> The value the array is stored in. </typeparam>
public class SubArray<TValue> : IEnumerable<TValue>
{
	public readonly Ref<TValue[]> referencedArray;
	private int startIndex = 0, length;

	/// <summary>
	/// Where the <see cref="SubArray{TValue}"/> inclusively starts on.
	/// </summary>
	/// <value>
	/// A zero-based index for where the <see cref="SubArray{TValue}"/> starts.
	/// </value>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public int StartIndex
	{
		get => startIndex;
		set
		{
			if (value > referencedArray.Value.Length)
				throw new IndexOutOfRangeException($"{nameof(value)} {startIndex}"
					+ $" is larger than the referenced array of {referencedArray.Value.Length}");
			else if (value < 0)
				throw new IndexOutOfRangeException($"value cannot be below 0!");
			Length = length; // Property handling check
			startIndex = value;
		}
	}

	/// <summary>
	/// Where the <see cref="SubArray{TValue}"/> inclusively ends on.
	/// </summary>
	/// <value>
	/// A zero-based index for where the <see cref="SubArray{TValue}"/> ends.
	/// </value>
	/// <exception cref="ArgumentException">value cannot be below " +
	/// 	$"{nameof(StartIndex)} with {StartIndex}! - value</exception>
	public int EndIndex
	{
		get => startIndex + Length - 1;
		set => Length = value - startIndex + 1;
	}

	/// <summary>
	/// Converts <paramref name="relativeIndex"/> from how <see cref="SubArray{TValue}"/>
	/// handles the value to where the <see cref="Array"/> handles the value.
	/// Ignores any endIndex checks.
	/// </summary>
	/// <param name="relativeIndex">index for the <see cref="SubArray{TValue}"/></param>
	/// <returns>
	/// An absolute index from index of <see cref="SubArray{TValue}"/> to the 
	/// referenced Array.
	/// </returns>
	public int ToAbsoluteIndex(int relativeIndex)
		=> relativeIndex + startIndex;

	/// <summary>
	/// Gets or Sets total number of elements the subarray can access.
	/// </summary>
	/// <value>
	/// The amount of elements the <see cref="SubArray{TValue}"/> can access.
	/// </value>
	/// <exception cref="IndexOutOfRangeException">value cannot be below 0!</exception>
	public int Length
	{
		get => length;
		set
		{
			if (value - startIndex > referencedArray.Value.Length)
				throw new IndexOutOfRangeException($"{nameof(value)} {value + startIndex}"
					+ $" is larger than the referenced array of {referencedArray.Value.Length}");
			else if (value < 0)
				throw new IndexOutOfRangeException($"value cannot be below 0!");
			length = value;
		}
	}



	public SubArray(Ref<TValue[]> referencedArray, int startIndex) :
		this(referencedArray, startIndex, referencedArray.Value.Length - startIndex)
	{ }
	public SubArray(Ref<TValue[]> referencedArray, int startIndex, int length)
	{
		this.referencedArray = referencedArray;
		StartIndex = startIndex;
		Length = length;
	}

	public SubArray(SubArray<TValue> subArray, int startIndex) :
		this(subArray, startIndex, subArray.Length - startIndex)
	{ }
	public SubArray(SubArray<TValue> subArray, int startIndex, int length)
	{
		referencedArray = new Ref<TValue[]>(() => subArray.referencedArray.Value,
			setter => subArray.referencedArray.Value = setter);
		StartIndex = subArray.StartIndex + startIndex;
		Length = length;
	}


	/// <summary>
	/// Gets or sets the <typeparamref name="TValue"/> at the specified index, which is 
	/// recieved by original array.
	/// </summary>
	/// <value> The <typeparamref name="TValue"/> in the original array. </value>
	/// <param name="index">
	/// The index where the <see cref="SubArray{TValue}"/>
	/// accesses in.</param>
	/// <param name="asAbsolute">
	/// if set to <c>true</c>, then it won't adjust for <see cref="startIndex"/>
	/// </param>
	public TValue this[int index, bool asAbsolute = false]
	{
		get
		{
			if (!asAbsolute)
				index += startIndex;
			CheckRange(index);
			return referencedArray.Value[index];
		}
		set
		{
			if (!asAbsolute)
				index += startIndex;
			CheckRange(index);
			referencedArray.Value[index] = value;
		}
	}

	/// <summary>
	/// Checks the range of <paramref name="index"/>. If it is not in the specified
	/// space of <see cref="startIndex"/> and <see cref="EndIndex"/>, then it will
	/// throw an error. Otherwise does nothing.
	/// </summary>
	/// <param name="index">The input index.</param>
	/// <exception cref="IndexOutOfRangeException"></exception>
	private void CheckRange(int index)
	{
		if (index > EndIndex)
			throw new IndexOutOfRangeException($"{nameof(index)} {index} "
				+ $"is higher than {EndIndex}, as a subarray length of {Length}!");
		if (index < StartIndex)
			throw new IndexOutOfRangeException($"{nameof(index)} {index} "
				+ $"is lower than {startIndex}, as a subarray length of {Length}!");
	}

	IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
	{
		for (int i = StartIndex; i <= EndIndex; i++)
			yield return this[i, true];
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = StartIndex; i <= EndIndex; i++)
			yield return this[i, true];
	}
}