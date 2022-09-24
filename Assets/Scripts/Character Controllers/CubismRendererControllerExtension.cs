namespace B1NARY
{
	using Live2D.Cubism.Rendering;
	using System.Linq;
	using UnityEngine;

	// Inheriting from this script causes some minor code modification, but should
	// - be good to go.
	public class CubismRendererControllerExtension : CubismRenderController
	{
		public Color Color
		{
			get => Renderers.First().Color; set
			{
				if (Renderers.First().Color == value)
					return;
				for (int i = 0; i < Renderers.Length; i++)
				{
					Renderers[i].Color = value;
				}
			}
		}
	}
}

