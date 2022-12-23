namespace B1NARY.UI
{
	using DataPersistence;
	using UnityEngine;
	using UnityEngine.UI;

	public class SavePanel : AutoPagePopulator
	{
		protected override void Awake()
		{
			base.Awake();
			using (var enumerator = PersistentData.GetAllFiles().GetEnumerator())
				while (enumerator.MoveNext())
				{
					GameObject obj = AddEntry();
					RectTransform rectTransform = obj.GetComponent<RectTransform>();
					Image image = obj.GetComponentInChildren<Image>();
					Texture2D texture = enumerator.Current.Image;
					image.sprite = Sprite.Create(texture, rectTransform.rect, new Vector2(0.5f, 0.5f));
				}
		}
	}
}