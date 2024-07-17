namespace B1NARY.UI
{
	using UnityEngine;
	using TMPro;
	using System.Linq;
	using B1NARY.DesignPatterns;

	[RequireComponent(typeof(TMP_Text))]
	public sealed class DebugLineTester : Singleton<DebugLineTester>
	{
		public int lineLimit = 8;
		private TMP_Text text;
		protected override void SingletonAwake()
		{
			text = GetComponent<TMP_Text>();
			text.text = new string('\n', lineLimit);
			Application.logMessageReceived += (con, stack, type) => AddLine($"{type}: {con}, {stack.Split('\n').First()}");
		}

		public void AddLine(string line)
		{
			string[] text = this.text.text.Split('\n');
			for (int i = 0; i < text.Length - 1; i++)
			{
				text[i + 1] = text[i];
			}
			text[0] = line;
		}
	}
}