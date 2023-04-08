namespace B1NARY.UI.Colors
{
	using OVSXmlSerializer;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using UnityEngine;

	public partial class ColorFormat
	{
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
		public Color primaryUI = new Color(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		[XmlNamedAs("Secondary")]
		public Color SecondaryUI = new Color(ToPercent(47), ToPercent(206), ToPercent(172));
		/// <summary>
		/// 
		/// </summary>
		[XmlNamedAs("Extras")]
		public Dictionary<string, Color> ExtraUIColors = new Dictionary<string, Color>();

		// OTHER STRUCTS -------------------

		

		public void Save(string fileName, bool isDefault = false)
		{
			if (isDefault)
			{
				using (var stream = DefaultThemePath.Open(FileMode.Create, FileAccess.Write))
					FormatSerializer.Serialize(stream, this, "DefaultFormat");
				return;
			}
			using (var stream = CustomThemePath.GetFile(fileName).Open(FileMode.Create, FileAccess.Write))
				FormatSerializer.Serialize(stream, this, "ColorFormat");
		}
	}
}