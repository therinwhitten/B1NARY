namespace HDConsole
{
	using OVSSerializer.IO;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;

	public class LogAutoSyncer : IDisposable
	{
		private readonly HDConsole console;
		private readonly OSFile targetFile;
		private readonly Thread thread;
		private readonly List<string> newLines = new();
		private bool disposed = false;
		public LogAutoSyncer(OSFile fileInfo, HDConsole console, TimeSpan timerDelay)
		{
			this.console = console;
			this.targetFile = fileInfo;
			thread = new(Loop) { Priority = ThreadPriority.Lowest };
			console.AddedLine += NewLine;
			File.WriteAllText(targetFile.FullPath, "");
			thread.Start();

			void Loop()
			{
			loopbreak:
				Thread.Sleep(timerDelay);
				if (disposed)
					return;
				Update();
				goto loopbreak;
			}
		}
		private void NewLine(string line) => newLines.Add(line);
		public void Update()
		{
			if (newLines.Count == 0)
				return;
			_ = File.AppendAllLinesAsync(targetFile.FullPath, newLines.ToArray());
			newLines.Clear();
		}
		public void Dispose()
		{
			console.AddedLine -= NewLine;
			disposed = true;
			Update();
		}
	}
}
