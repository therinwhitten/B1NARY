namespace B1NARY
{
	using System;
	
	/// <summary>
	/// An extension of <see cref="DateTime"/> that keeps track of important,
	/// recognisible events.
	/// </summary>
	public static class DateTimeTracker
	{
		/// <summary>
		/// If the day today is during halloween.
		/// </summary>
		public static bool IsHalloween { get; }
		/// <summary>
		/// If the day today is april fools day.
		/// </summary>
		public static bool IsAprilFools { get; }
			
		/// <summary>
		/// Uses <see cref="DateTime.Today"/> to capture the value that is stored
		/// once per game session; Does not change.
		/// </summary>
		public static DateTime CapturedDateTime { get; }
		static DateTimeTracker()
		{
			CapturedDateTime = DateTime.Today;
			IsHalloween = CapturedDateTime.Month == 10 && CapturedDateTime.Day > 15;
			IsAprilFools = CapturedDateTime.Month == 4 && CapturedDateTime.Day == 1;
		}
	}
}