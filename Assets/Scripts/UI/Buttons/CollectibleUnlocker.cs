namespace B1NARY
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public class CollectibleUnlocker : MonoBehaviour
	{
		public string category = $"{CollectibleCollection.UNLOCKED_GALLERY_KEY}/{CollectibleCollection.UNLOCKED_MAP_KEY}/{CollectibleCollection.UNLOCKED_CHAR_KEY}";
		public string collectibleName = "Doomsucker";

		public void GiveCollectible()
		{
			CollectibleCollection.UnlockUnlockable(category, collectibleName);
		}
	}
}
