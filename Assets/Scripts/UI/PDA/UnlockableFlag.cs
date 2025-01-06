namespace B1NARY
{
	using B1NARY.DataPersistence;
	using B1NARY.Globalization;
	using OVSSerializer;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;

	[Serializable]
	public class UnlockableFlag
	{
		#region Serialization
		public static UnlockableFlag FromString(string text)
		{
			string[] split = text.Split('/');
			if (!bool.TryParse(split[0], out bool resultBool))
				throw new FormatException($"Failed to serialize bool from '{text}'!");
			return new UnlockableFlag
			{
				flag = split[2],
				category = split[1],
				active = resultBool,
			};
		}
		public override string ToString()
		{
			return $"{!active}/{category}/{flag}";
		}
		#endregion

		public string FlagName { get => flag; set { flag = value; FlagNameChanged?.Invoke(value); } }
		public event Action<string> FlagNameChanged;
		public string Category { get => category; set { category = value; CategoryChanged?.Invoke(value); } }
		public event Action<string> CategoryChanged;
		public bool IsActive { get => active; set { active = value; ActiveChanged?.Invoke(value); } }
		public event Action<bool> ActiveChanged;

		[OVSXmlText]
		private string flag;
		[OVSXmlAttribute]
		private string category;
		[OVSXmlAttribute]
		private bool active;


		private static readonly Regex nameRegex = new(@"([a-z])([A-Z])");
		/// <summary>
		/// Translates the <see cref="flag"/> to readable text for end users.
		/// </summary>
		public void Globalize(TextboxGlobalizer textBox)
		{
			textBox.SetText(flag, nameRegex.Replace(flag, "$1 $2"));
		}
	}
}
