namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public static class ResolutionUtility
	{
		public static IReadOnlyList<Resolution> OrderedResolutions { get; } = GetResolutions()
			.OrderByDescending(resolution => (resolution.width * 100) + resolution.height)
			.ToArray();

		public static List<Resolution> GetResolutions()
		{
			double highestRefreshRate = 0f;
			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				double comparator = Screen.resolutions[i].refreshRateRatio.value;
				if (highestRefreshRate < comparator)
					highestRefreshRate = comparator;
			}

			List<Resolution> output = new();
			for (int i = 0; i < Screen.resolutions.Length; i++)
			{
				Resolution resolution = Screen.resolutions[i];
				if (resolution.refreshRateRatio.value != highestRefreshRate)
					continue;
				output.Add(resolution);
			}

			return output;
		}
	}
}