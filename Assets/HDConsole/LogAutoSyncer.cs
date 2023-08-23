namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;
	using IO;

	public class LogAutoSyncer : IDisposable
	{
		private readonly HDConsole console;
		private readonly OSFile targetFile;
		private readonly Timer timer;
		private readonly List<string> newLines = new();
		public LogAutoSyncer(OSFile fileInfo, HDConsole console, TimeSpan timerDelay)
		{
			this.console = console;
			this.targetFile = fileInfo;
			timer = new Timer(Update, null, timerDelay, timerDelay);
			console.AddedLine += NewLine;
			File.WriteAllText(targetFile.FullName, "");
		}
		private void NewLine(string line) => newLines.Add(line);
		public void Update(object state = null)
		{
			if (newLines.Count == 0)
				return;
			_ = File.AppendAllLinesAsync(targetFile.FullName, newLines.ToArray());
			newLines.Clear();
		}
		public void Dispose()
		{
			console.AddedLine -= NewLine;
			Update();
			timer.Dispose();
		}
	}
}
