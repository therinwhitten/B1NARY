namespace B1NARY.UI.Colors
{
	using B1NARY.IO;
	using OVSXmlSerializer;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using UnityEngine;

	public partial class ColorFormat
	{
		public const string ROOT_NAME_CUSTOM = "ColorFormat",
			ROOT_NAME_DEFAULT = "DefaultFormat";
		public const string COLOR_NAME_PRIMARY = "Primary",
			COLOR_NAME_SECONDARY = "Secondary";

		public XmlSerializer<ColorFormat> FormatSerializer => XmlSerializer<ColorFormat>.Default;
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("name")]
		public string FormatName;
		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		[XmlNamedAs("Primary")]
		public Color primaryUI = new(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		[XmlNamedAs("Secondary")]
		public Color SecondaryUI = new(ToPercent(47), ToPercent(206), ToPercent(172));
		/// <summary>
		/// 
		/// </summary>
		[XmlNamedAs("Extras")]
		public Dictionary<string, Color> ExtraUIColors = new();

		// OTHER STRUCTS -------------------

		public bool TryGetColor(string name, out Color color)
		{
			if (COLOR_NAME_PRIMARY == name)
			{
				color = primaryUI;
				return true;
			}
			else if (COLOR_NAME_SECONDARY == name)
			{
				color = SecondaryUI;
				return true;
			}
			else if (ExtraUIColors.TryGetValue(name, out color))
				return true;
			color = primaryUI;
			return false;
		}

		public void Save(string fileName, bool isDefault = false)
		{
			if (isDefault)
			{
				using FileStream defaultStream = DefaultThemePath.Create();
				FormatSerializer.Serialize(defaultStream, this, ROOT_NAME_DEFAULT);
				return;
			}
			using FileStream stream = CustomThemePath.GetFile(fileName).Create();
			FormatSerializer.Serialize(stream, this, ROOT_NAME_CUSTOM);
		}
	}
}