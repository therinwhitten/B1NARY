namespace B1NARY
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	public sealed class SaveHider : MonoBehaviour
	{
		public static Thumbnail GetThumbnail()
		{
			Thumbnail output = null;
			try
			{
				output = Thumbnail.CreateWithScreenshot(128, 128);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return output;
		}
	}
}