namespace B1NARY
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	/// <summary>
	/// A serializable struct that handles the resolutions of the unity game.
	/// </summary>
	[Serializable]
	public readonly struct B1NARYResolution : IEquatable<B1NARYResolution>, IEquatable<Resolution>
	{
		/// <summary>
		/// Gets or sets the active resolution that the application is using.
		/// </summary>
		public static B1NARYResolution ActiveResolution
		{
			get => (B1NARYResolution)Screen.currentResolution;
			set => Screen.SetResolution(value.Width, value.Height, value.FullScreenMode, value.RefreshRate);
		}
		/// <summary>
		/// A enumerated resolutions that is supported via fullscreen by the monitor
		/// </summary>
		public static IEnumerable<B1NARYResolution> MonitorResolutions { get; } = Screen.resolutions.Select(resolution => (B1NARYResolution)resolution);
		
		/// <summary>
		/// Initializes the game to use the active resolution on startup
		/// </summary>
		[RuntimeInitializeOnLoadMethod]
		private static void Init()
		{
			return;
			ActiveResolution = B1NARYConfig.Graphics.Resolution;
		}


		/// <summary>
		/// Creates a new <see cref="B1NARYResolution"/>, internally calls 
		/// <see cref="B1NARYResolution(Resolution)"/>
		/// </summary>
		/// <param name="resolution"></param>
		public static explicit operator B1NARYResolution(Resolution resolution) 
			=> new B1NARYResolution(resolution);
		/// <summary>
		/// Creates a new <see cref="Resolution"/> from <see cref="B1NARYResolution"/>,
		/// easy to use outside of the struct.
		/// </summary>
		public static implicit operator Resolution(B1NARYResolution resolution)
		{
			Resolution output = default;
			output.height = resolution.Height;
			output.width = resolution.Width;
			output.refreshRate = resolution.RefreshRate;
			return output;
		}

		/// <summary>
		/// The height of the application in pixels.
		/// </summary>
		public int Height { get; }
		/// <summary>
		/// The width of the application in pixels.
		/// </summary>
		public int Width { get; }
		/// <summary>
		/// How many frames are to be expected to load in per second.
		/// </summary>
		public int RefreshRate { get; }
		/// <summary>
		/// The mode of the full screen.
		/// </summary>
		public FullScreenMode FullScreenMode { get; }

		/// <summary>
		/// Creates a new instance, borrowing the data from the resolution and
		/// taking a value from <see cref="Screen.fullScreenMode"/>.
		/// </summary>
		/// <param name="resolution"> The source. </param>
		public B1NARYResolution(Resolution resolution)
		{
			Height = resolution.height;
			Width = resolution.width;
			RefreshRate = resolution.refreshRate;
			FullScreenMode = Screen.fullScreenMode;
		}
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public B1NARYResolution(int height, int width, int refreshRate, FullScreenMode fullScreenMode)
		{
			Height = height;
			Width = width;
			RefreshRate = refreshRate;
			FullScreenMode = fullScreenMode;
		}

		public override string ToString() => $"{Width}x{Height}";

		public bool Equals(B1NARYResolution other)
		{
			return Height == other.Height 
				&& Width == other.Width 
				&& RefreshRate == other.RefreshRate 
				&& FullScreenMode == other.FullScreenMode;
		}

		public bool Equals(Resolution other)
		{
			return Height == other.height 
				&& Width == other.width 
				&& RefreshRate == other.refreshRate;
		}
	}
}