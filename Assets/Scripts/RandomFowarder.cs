using System;
using CRandom = System.Random;
using URandom = UnityEngine.Random;

// This will probably be used for future easter eggs to come by, so ill leave
// - this code here.
public static class RandomFowarder
{
	public static uint CSharpRandomIterations { get; private set; } = 0;
	public static uint UnityRandomIterations { get; private set; } = 0;
	private static CRandom randomCSharp = new CRandom();


	public enum RandomType
	{
		CSharp,
		Unity,
		Doom
	}
	public static int Next(RandomType randomType = RandomType.Unity)
		=> Next(int.MaxValue, randomType);
	public static int Next(int maxValue, RandomType randomType = RandomType.Unity)
		=> Next(0, maxValue, randomType);
	public static int Next(int minValue, int maxValue, RandomType randomType = RandomType.Unity)
	{
		switch (randomType)
		{
			case RandomType.CSharp: CSharpRandomIterations++; 
				return randomCSharp.Next(minValue, maxValue);
			case RandomType.Unity: UnityRandomIterations++; 
				return URandom.Range(minValue, maxValue);
			case RandomType.Doom:
				byte currentDoomValue = GetRawDoomRandom();
				int difference = maxValue - minValue;
				return currentDoomValue % difference;
		}
		throw new IndexOutOfRangeException();
	}

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
				throw new ArgumentException($"Doom as random type option" +
					$" does not have a good solution for floating point values.");
		}
		throw new IndexOutOfRangeException();
	}

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
				throw new ArgumentException($"Doom as random type option" +
					$" does not have a good solution for floating point values.");
		}
		throw new IndexOutOfRangeException();
	}

	public static float NextRange(float maxValue, float minPercent, float maxPercent, RandomType randomType)
	{
		minPercent *= maxValue; maxPercent *= maxValue;
		maxValue -= maxPercent - minPercent;
		float difference = maxPercent - maxValue;
		switch (randomType) // Damn you primitive version of C# and switch expressions!
		{
			case RandomType.CSharp:
				return ((float)randomCSharp.NextDouble() * difference) + maxValue;
			case RandomType.Unity:
				return (URandom.value * difference) + maxValue;
			case RandomType.Doom:
				throw new ArgumentException($"Doom as random type option" +
				$" does not have a good solution for floating point values.");
			default:
				throw new NotImplementedException(randomType.ToString());
		}
	}

	// This handles the optional doom random number gen for enthusiasts
	public static uint DoomRandomIterations { get; private set; } = 0;
	// Unchecked intentionally
	public static byte DoomIndex { get; private set; } = 0;
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
	public static void ResetDoomRandomIndex() => DoomIndex = 0;
	public static byte GetRawDoomRandom()
	{
		DoomIndex++;
		DoomRandomIterations++;
		return doomRandomTable[DoomIndex];
	}
	public static int DoomNext(int startValue, int multiples, int iterations)
	{
		byte currentDoomValue = GetRawDoomRandom();
		int randomValue = currentDoomValue % iterations;
		randomValue *= multiples;
		return startValue + randomValue;
	}
}