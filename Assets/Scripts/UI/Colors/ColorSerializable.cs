namespace B1NARY.DataPersistence
{
	using System;
	using UnityEngine;
	using B1NARY.UI;

	[Serializable]
	public struct ColorSerializable
	{
		public static explicit operator ColorSerializable(Color color)
		{
			//Debug.Log($"Boxing Color: {color.r}, {color.g}, {color.b}, {color.a}");
			return new ColorSerializable(color);
		}

		public static explicit operator Color(ColorSerializable color)
		{
			//Debug.Log($"Unboxing Color: {color.red}, {color.green}, {color.blue}, {color.alpha}");
			return new Color(ColorFormat.ToPercent(color.red), ColorFormat.ToPercent(color.green),
				ColorFormat.ToPercent(color.blue), ColorFormat.ToPercent(color.alpha));
		}

		public byte red, green, blue, alpha;
		private ColorSerializable(Color color)
		{
			red = ColorFormat.ToByte(color.r);
			green = ColorFormat.ToByte(color.g);
			blue = ColorFormat.ToByte(color.b);
			alpha = ColorFormat.ToByte(color.a);
		}
		public ColorSerializable(byte red, byte green, byte blue, byte alpha)
		{
			this.red = red;
			this.green = green;
			this.blue = blue;
			this.alpha = alpha;
		}
		public override string ToString() =>
			$"Boxed Color: {red}, {green}, {blue}, {alpha}";
	}
}