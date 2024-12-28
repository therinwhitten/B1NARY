namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.Events;

	[RequireComponent(typeof(CanvasGroup))]
	public class LockBoxToggler : MonoBehaviour
	{
		private CanvasGroup _canvasGroup;
		public CanvasGroup CanvasGroup => _canvasGroup == null ? _canvasGroup = GetComponent<CanvasGroup>() : _canvasGroup;

		[SerializeField]
		public string FlagName = "PlaceholderFlagName";
		[SerializeField]
		internal EnumType FlagGroupType = EnumType.Gallery;
		internal enum EnumType : byte { Gallery, Map, CharacterProfile }
		[SerializeField]
		public bool HideIfFlagExists = true;

		[SerializeField]
		public UnityEvent<bool> OnEnableFlagExists = new();


		public HashSet<string> Flags
		{
			get
			{
				if (_flags is not null)
					return _flags;
				PlayerConfig.Instance.collectibles.MergeSavesFromSingleton();
				var merger = PlayerConfig.Instance.collectibles;
				_flags = FlagGroupType switch
				{
					EnumType.Map => merger.Map,
					EnumType.CharacterProfile => merger.CharacterProfiles,
					_ => merger.Gallery,
				};
				return _flags;
			}
		}
		private HashSet<string> _flags;

		public bool HasFlag => Flags.Contains(FlagName);


		private void OnEnable()
		{
			UpdateCanvasGroup();
			OnEnableFlagExists.Invoke(HasFlag);
		}
		private void OnDisable()
		{
			_flags = null;
		}

		// Has to be a separate togglable gameobject to avoid same-sync loading times
		public void UpdateCanvasGroup()
		{
			bool hide = HideIfFlagExists && HasFlag;
			CanvasGroup.blocksRaycasts = !hide;
			CanvasGroup.alpha = hide ? 0f : 1f;
			CanvasGroup.interactable = !hide;
		}
	}
}