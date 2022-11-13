namespace B1NARY
{
	using Live2D.Cubism.Rendering;
	using UnityEngine;

	// Inheriting from this script causes some minor code modification, but should
	// - be good to go.
	public class CubismRendererControllerExtension : CubismRenderController
	{
		public Color Color
		{
			get => Renderers[0].Color; set
			{
				if (Renderers[0].Color == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].Color = value;
				}
			}
		}
		public Color MultiplyColor
		{
			get => Renderers[0].MultiplyColor; set
			{
				if (Renderers[0].MultiplyColor == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].MultiplyColor = value;
				}
			}
		}
		public Color ScreenColor
		{
			get => Renderers[0].ScreenColor; set
			{
				if (Renderers[0].ScreenColor == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].ScreenColor = value;
				}
			}
		}
		public Material Material
		{
			get => Renderers[0].Material; set
			{
				if (Renderers[0].Material == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].Material = value;
				}
			}
		}
	}
}

