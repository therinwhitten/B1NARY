namespace B1NARY
{
	using System;
	using UnityEngine;
	using CRandom = System.Random;
	using URandom = UnityEngine.Random;

	/// <summary> The primary class used for randomization. </summary>
	public static class RandomForwarder
	{
		public static T Random<T>(this T[] values, RandomType type = RandomType.Unity)
		{
			return values[Next(values.Length, type)];
		}

		private static int SetNewRandomSeed() => m_currentSeed = DateTime.Now.Millisecond;
		/// <summary> 
		/// Count of how many iterations the <see cref="RandomMaster"/> was used
		/// from <see cref="RandomType.CSharp"/>. Purely cosmetic by default.
		/// </summary>
		public static uint CSharpRandomIterations { get; private set; } = 0;

		/// <summary> 
		/// Count of how many iterations the <see cref="RandomMaster"/> was used
		/// from <see cref="RandomType.Unity"/>. Purely cosmetic by default.
		/// </summary>
		public static uint UnityRandomIterations { get; private set; } = 0;
		private static int m_currentSeed;
		private static CRandom randomCSharp = new CRandom(SetNewRandomSeed());
		public static int CurrentSeed { get => m_currentSeed; set => randomCSharp = new CRandom(value); }

		/// <summary>
		/// The Type of Randomization the <see cref="RandomMaster"/> will use.
		/// </summary>
		public enum RandomType
		{
			/// <summary> The C-Sharp type of randomization. </summary>
			[InspectorName("C#")]
			CSharp,
			/// <summary> Default. Unity type of randomization. </summary>
			Unity,
			/// <summary> Doom's unique type of randomization, from 1993. </summary>
			[InspectorName("Doom (1993)")]
			Doom
		}

		/// <summary>
		/// Gets a random value from 0 to <see cref="int.MaxValue"/>.
		/// </summary>
		/// <param name="randomType">The random type for randomization. </param>
		/// <returns> A random int value. </returns>
		public static int Next(RandomType randomType = RandomType.Unity)
			=> Next(int.MaxValue, randomType);

		/// <summary>
		/// Gets a random value from 0 to <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="maxValue"> The maximum value for randomization. </param>
		/// <param name="randomType">The random type for randomization. </param>
		/// <returns> A random int value. </returns>
		public static int Next(int maxValue, RandomType randomType = RandomType.Unity)
			=> Next(0, maxValue, randomType);

		/// <summary>
		/// Gets a random value from <paramref name="minValue"/> to <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="minValue"> The minimum value for randomization. </param>
		/// <param name="maxValue"> The maximum value for randomization. </param>
		/// <param name="randomType">The random type for randomization. </param>
		/// <returns> A random int value. </returns>
		public static int Next(int minValue, int maxValue, RandomType randomType = RandomType.Unity)
		{
			switch (randomType)
			{
				case RandomType.CSharp:
					CSharpRandomIterations++;
					return randomCSharp.Next(minValue, maxValue);
				case RandomType.Unity:
					UnityRandomIterations++;
					return URandom.Range(minValue, maxValue);
				case RandomType.Doom:
					byte currentDoomValue = GetRawDoomRandom();
					int difference = maxValue - minValue;
					return currentDoomValue % difference;
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Gets a value from 0 to 1 as a <see cref="float"/>.
		/// </summary>
		/// <param name="randomType">The random type for randomization. </param>
		/// <returns> A random <see cref="float"/> from 0 to 1. </returns>
		public static float NextFloat(RandomType randomType = RandomType.Unity)
		{
			switch (randomType)
			{
				case RandomType.CSharp:
					CSharpRandomIterations++;
					return (float)randomCSharp.NextDouble();
				case RandomType.Unity:
					UnityRandomIterations++;
					return URandom.value;
				case RandomType.Doom:
					DoomRandomIterations++;
					return (float)GetRawDoomRandom() / byte.MaxValue;
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Gets a value from 0 to 1 as a <see cref="double"/>.
		/// </summary>
		/// <param name="randomType">The random type for randomization. </param>
		/// <returns> A random <see cref="double"/> from 0 to 1. </returns>
		public static double NextDouble(RandomType randomType = RandomType.CSharp)
		{
			switch (randomType)
			{
				case RandomType.CSharp:
					CSharpRandomIterations++;
					return randomCSharp.NextDouble();
				case RandomType.Unity:
					UnityRandomIterations++;
					return URandom.value;
				case RandomType.Doom:
					return (double)GetRawDoomRandom() / byte.MaxValue;
			}
			throw new IndexOutOfRangeException();
		}


		public static float NextRange(float maxValue, float minPercent, float maxPercent, RandomType randomType)
		{
			minPercent *= maxValue;
			maxPercent *= maxValue;
			maxValue -= maxPercent - minPercent;
			float difference = maxPercent - maxValue;
			switch (randomType)
			{
				case RandomType.CSharp:
					return (NextFloat(RandomType.CSharp) * difference) + maxValue;
				case RandomType.Unity:
					return (NextFloat(RandomType.Unity) * difference) + maxValue;
				case RandomType.Doom:
					return (NextFloat(RandomType.Doom) * difference) + maxValue;
			}
			return float.NaN;
		}

		/// <summary> 
		/// Count of how many iterations the <see cref="RandomMaster"/> was used
		/// from <see cref="RandomType.Doom"/>. Purely cosmetic by default.
		/// </summary>
		public static uint DoomRandomIterations { get; private set; } = 0;

		/// <summary>
		/// The current index of the doom random value table, intentionally unchecked.
		/// </summary>
		public static byte DoomIndex { get; private set; } = 0;

		/// <summary>
		/// Doom's randomized table primarily used by <see cref="DoomIndex"/>.
		/// </summary>
		public static readonly byte[] doomRandomTable =
		{
			0,   8, 109, 220, 222, 241, 149, 107,  75, 248, 254, 140,  16,  66 ,
			74,  21, 211,  47,  80, 242, 154,  27, 205, 128, 161,  89,  77,  36 ,
			95, 110,  85,  48, 212, 140, 211, 249,  22,  79, 200,  50,  28, 188 ,
			52, 140, 202, 120,  68, 145,  62,  70, 184, 190,  91, 197, 152, 224 ,
			149, 104,  25, 178, 252, 182, 202, 182, 141, 197,   4,  81, 181, 242 ,
			145,  42,  39, 227, 156, 198, 225, 193, 219,  93, 122, 175, 249,   0 ,
			175, 143,  70, 239,  46, 246, 163,  53, 163, 109, 168, 135,   2, 235 ,
			25,  92,  20, 145, 138,  77,  69, 166,  78, 176, 173, 212, 166, 113 ,
			94, 161,  41,  50, 239,  49, 111, 164,  70,  60,   2,  37, 171,  75 ,
			136, 156,  11,  56,  42, 146, 138, 229,  73, 146,  77,  61,  98, 196 ,
			135, 106,  63, 197, 195,  86,  96, 203, 113, 101, 170, 247, 181, 113 ,
			80, 250, 108,   7, 255, 237, 129, 226,  79, 107, 112, 166, 103, 241 ,
			24, 223, 239, 120, 198,  58,  60,  82, 128,   3, 184,  66, 143, 224 ,
			145, 224,  81, 206, 163,  45,  63,  90, 168, 114,  59,  33, 159,  95 ,
			28, 139, 123,  98, 125, 196,  15,  70, 194, 253,  54,  14, 109, 226 ,
			71,  17, 161,  93, 186,  87, 244, 138,  20,  52, 123, 251,  26,  36 ,
			17,  46,  52, 231, 232,  76,  31, 221,  84,  37, 216, 165, 212, 106 ,
			197, 242,  98,  43,  39, 175, 254, 145, 190,  84, 118, 222, 187, 136 ,
			120, 163, 236, 249
		};
		/// <summary> Resets <see cref="DoomIndex"/> to default. </summary>
		public static void ResetDoomRandomIndex() => DoomIndex = 0;

		/// <summary>
		/// Gets a <see cref="byte"/> from <see cref="doomRandomTable"/> and <see cref="DoomIndex"/>.
		/// </summary>
		/// <returns> A 'random' <see cref="byte"/>. </returns>
		public static byte GetRawDoomRandom()
		{
			DoomIndex++;
			DoomRandomIterations++;
			return doomRandomTable[DoomIndex];
		}

		/// <summary>
		/// Gets a value in the style of Doom's 1993 randomization.
		/// </summary>
		/// <param name="startValue">The minimum value the <see cref="int"/> has.</param>
		/// <param name="multiples"> The value of each one of <paramref name="iterations"/>.</param>
		/// <param name="iterations">How many random iterations it can be repeated. </param>
		/// <returns></returns>
		public static int DoomNext(int startValue, int multiples, int iterations)
		{
			byte currentDoomValue = GetRawDoomRandom();
			int randomValue = currentDoomValue % iterations;
			randomValue *= multiples;
			return startValue + randomValue;
		}
	}
}