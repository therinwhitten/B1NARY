namespace B1NARY.UI.Colors
{
	using OVSSerializer.IO;
	using OVSSerializer;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using UnityEngine;

	public partial class ColorFormat
	{
		public const string ROOT_NAME_CUSTOM = "ColorFormat";
		public const string COLOR_NAME_PRIMARY = "Primary",
			COLOR_NAME_SECONDARY = "Secondary";

		public static OVSXmlSerializer<ColorFormat> FormatSerializer => OVSXmlSerializer<ColorFormat>.Shared;
		/// <summary>
		/// 
		/// </summary>
		[OVSXmlAttribute("name")]
		public string FormatName;
		public bool IsPlayableFormat => playerControlled || IsDefault;
		/// <summary>
		/// If the value can be controlled by the player. This is always true for
		/// player-customized formats.
		/// </summary>
		[OVSXmlNamedAs("PlayerControlled")]
		internal bool playerControlled = false;
		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		[OVSXmlNamedAs("Primary")]
		public Color PrimaryUI = new(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="PrimaryUI"/> ying.
		/// </summary>
		[OVSXmlNamedAs("Secondary")]
		public Color SecondaryUI = new(ToPercent(47), ToPercent(206), ToPercent(172));
		/// <summary>
		/// 
		/// </summary>
		[OVSXmlNamedAs("Extras")]
		public Dictionary<string, Color> ExtraUIColors = new();
		public bool IsDefault => FormatName == DEFAULT_THEME_NAME;

		// OTHER STRUCTS -------------------

		public bool TryGetColor(string name, out Color color)
		{
			if (COLOR_NAME_PRIMARY == name)
			{
				color = PrimaryUI;
				return true;
			}
			else if (COLOR_NAME_SECONDARY == name)
			{
				color = SecondaryUI;
				return true;
			}
			else if (ExtraUIColors.TryGetValue(name, out color))
				return true;
			color = PrimaryUI;
			return false;
		}

		/// <summary>
		/// Saves the color format into the streaming assets folder.
		/// </summary>
		/// <remarks>
		/// This assumes that this is the developer folder being used; You should
		/// not use it outside of that context.
		/// </remarks>
		public void Save()
		{
			using FileStream stream = RootPath.GetFile($"{FormatName}.xml").Create();
			FormatSerializer.Serialize(stream, this);
			ResetCache();
		}
		public void Delete()
		{
			File.Delete(RootPath.GetFile($"{FormatName}.xml").FullPath);
			ResetCache();
		}
		public string SaveFromPlayer()
		{
			OSFile file = PlayerThemePath.GetFile($"{FormatName}.xml");
			using FileStream stream = file.Create();
			FormatSerializer.Serialize(stream, this);
			ResetCache();
			return file.FullPath;
		}


		public override string ToString()
		{
			StringBuilder builder = new($"{FormatName}: {ToColorString(PrimaryUI)}-{ToColorString(SecondaryUI)}");
			if (ExtraUIColors.Count > 0)
			{
				builder.Append(" { ");
				builder.AppendJoin(", ", ExtraUIColors.Select(pair => $"{pair.Key}={ToColorString(pair.Value)}"));
				builder.Append(" }");
			}
			return builder.ToString();

			static string ToColorString(Color color) => color.a == 1f ? $"#{ColorUtility.ToHtmlStringRGB(color)}" : $"#{ColorUtility.ToHtmlStringRGBA(color)}";
		}
	}
}