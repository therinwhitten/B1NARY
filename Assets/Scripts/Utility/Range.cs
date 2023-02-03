namespace B1NARY
{
	using System;
	using UnityEngine;

	/// <summary>
	///  A range with a stored int value inside.
	/// </summary>
	[Serializable]
	public class StoredIntRange
	{
		public int Value
		{
			get => m_value;
			set
			{
				m_value = Mathf.Clamp(value, Range.Start, Range.End);
			}
		}
		private int m_value;
		public RangeInt Range;

		public StoredIntRange(int value, RangeInt rangeInt)
		{
			m_value = 0;
			Range = rangeInt;
			Value = value;
		}
	}
	/// <summary>
	///  A range with a stored float value inside.
	/// </summary>
	[Serializable]
	public class StoredRange
	{
		public float Value
		{
			get => m_value;
			set
			{
				m_value = Mathf.Clamp(value, Range.Start, Range.End);
			}
		}
		private float m_value;
		public Range Range;

		public StoredRange(float value, Range rangeInt)
		{
			m_value = 0;
			Range = rangeInt;
			Value = value;
		}
	}
	/// <summary>
	/// A serializable range that is both inclusive for integers.
	/// </summary>
	[Serializable]
	public struct RangeInt : IEquatable<RangeInt>
	{
		/// <summary>
		/// Effectively what <see cref="Count"/> does: Makes sure if it is 0
		/// or larger to apply as a range.
		/// </summary>
		/// <param name="startInclusive"> The inclusive starting value. </param>
		/// <param name="endInclusive"> The inclsuive end value. </param>
		/// <returns> If it doesn't end with a count less than 0. </returns>
		private static bool MatchesConditions(int startInclusive, int endInclusive)
		{
			return startInclusive + endInclusive - 1 >= 0;
		}
		/// <summary>
		/// The start of the range, inclusive.
		/// </summary>
		public int Start
		{
			get => m_start;
			set
			{
				if (!MatchesConditions(value, End))
					throw new InvalidOperationException();
				m_start = value;
			}
		}
		private int m_start;

		/// <summary>
		/// The end of the range, inclusive.
		/// </summary>
		public int End
		{
			get => m_end;
			set
			{
				if (!MatchesConditions(Start, value))
					throw new InvalidOperationException();
				m_end = value;
			}
		}
		private int m_end;
		/// <summary>
		/// The count between the numbers.
		/// </summary>
		public int Count
		{
			get => End - Start + 1; // Both values are inclusive, so added 1
			set => End = Start + value - 1;
		}

		public RangeInt(int startInclusive, int endInclusive) :this(startInclusive, endInclusive, 0)
		{

		}
		public RangeInt(int startInclusive, int endInclusive, int value)
		{
			if (!MatchesConditions(startInclusive, endInclusive))
				throw new InvalidOperationException();
			m_start = startInclusive;
			m_end = endInclusive;
		}

		public override string ToString() => $"{nameof(RangeInt)} {{{Start} to {End}, Amount {Count}}}";

		public bool Equals(RangeInt other)
		{
			return Start == other.Start &&
				End == other.End;
		}
	}

	/// <summary>
	/// A serializable range that is both inclusive for integers.
	/// </summary>
	[Serializable]
	public struct Range : IEquatable<Range>
	{
		/// <summary>
		/// Effectively what <see cref="Amount"/> does: Makes sure if it is 0
		/// or larger to apply as a range.
		/// </summary>
		/// <param name="startInclusive"> The inclusive starting value. </param>
		/// <param name="endInclusive"> The inclsuive end value. </param>
		/// <returns> If it doesn't end with a count less than 0. </returns>
		private static bool MatchesConditions(float startInclusive, float endInclusive)
		{
			return startInclusive + endInclusive >= 0;
		}
		/// <summary>
		/// The start of the range, inclusive.
		/// </summary>
		public float Start
		{
			get => m_start;
			set
			{
				if (!MatchesConditions(value, End))
					throw new InvalidOperationException();
				m_start = value;
			}
		}
		private float m_start;

		/// <summary>
		/// The end of the range, inclusive.
		/// </summary>
		public float End
		{
			get => m_end;
			set
			{
				if (!MatchesConditions(Start, value))
					throw new InvalidOperationException();
				m_end = value;
			}
		}
		private float m_end;

		/// <summary>
		/// The amount between the numbers.
		/// </summary>
		public float Amount
		{
			get => End - Start; // Both values are inclusive, so added 1
			set => End = Start + value;
		}

		public Range(float startInclusive, float endInclusive)
		{
			if (!MatchesConditions(startInclusive, endInclusive))
				throw new InvalidOperationException();
			m_start = startInclusive;
			m_end = endInclusive;
		}

		public override string ToString() => $"{nameof(Range)} {{{Start} to {End}, Amount {Amount}}}";

		public bool Equals(Range other)
		{
			return Start == other.Start &&
				End == other.End;
		}
	}
}