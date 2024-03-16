#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using B1NARY.CharacterManagement;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	public sealed class CharacterListViewer : EditorWindow
	{
		public static bool CharacterManagerExists => CharacterManager.HasInstance;
		public static bool HasCharacters => CharacterManager.Instance.CharactersInScene.Count > 0;
		public static bool HasActiveCharacter => CharacterManager.Instance.ActiveCharacter.HasValue;
		public static Character ActiveCharacter => CharacterManager.Instance.ActiveCharacter.Value;

		[MenuItem("B1NARY/Character List Viewer")]
		public static CharacterListViewer ShowWindow()
		{
			CharacterListViewer viewer = GetWindow<CharacterListViewer>();
			viewer.titleContent.text = "Character List Viewer";
			return viewer;
		}

		public void CreateGUI()
		{
			ShowActiveCharacter();
		}

		

		private bool ShowActiveCharacter()
		{
			if (!CharacterManagerExists)
				return false;
			if (!HasActiveCharacter)
				return false;
			Rect fullRect = position;
			fullRect.x = 0;
			fullRect.y = 0;
			fullRect.width = 250f;
			ShowCharacter(ActiveCharacter.controller, fullRect);
			return true;
		}

		private void ShowCharacter(IActor characterController, Rect rect)
		{
			Rect titleRect = rect;
			titleRect.height = 20f;
			var titleLabel = new Label($"<b>{characterController.CharacterNames.CurrentName} ({characterController.GameObjectName})</b>") { tooltip = "This contains the character name or in-game name, and the GameObject name, the name that will be referenced by certain scripts (like saving & loading) and in the scene" };
			//titleLabel.style.backgroundColor = new Color(0.2165094f, 0, 0.254717f);
			rootVisualElement.Add(titleLabel);
		}
	}
}
#endif