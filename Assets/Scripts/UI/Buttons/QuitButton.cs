namespace B1NARY.UI
{
	using System;
	using B1NARY;
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Button))]
	public class QuitButton : MonoBehaviour
	{
		public const string quitButtonKey = "Quit Button";
		public static bool VerifyGameObject(GameObject gameObject, out Button outButton)
		{
			outButton = null;
			int childCount = gameObject.transform.childCount;
			if (childCount <= 0)
				return false;
			for (int i = 0; i < childCount; i++)
			{
				GameObject @object = gameObject.transform.GetChild(childCount).gameObject;
				if (@object.name != quitButtonKey)
					continue;
				if (@object.TryGetComponent<Button>(out var button))
				{
					outButton = button;
					return true;
				}	
			}
			return false;
		}


		public bool askBeforeCommitting = false;
		public GameObject askPanel;
		public bool quitToScene = false;
		public string sceneName = "Main Menu";

		private Button button;

		private void Awake()
		{
			button = GetComponent<Button>();
		}
		private void OnEnable()
		{
			button.onClick.AddListener(QuitButtonBehaviour);
		}
		private void OnDisable()
		{
			button.onClick.RemoveListener(QuitButtonBehaviour);
		}

		private void QuitButtonBehaviour()
		{
			if (askBeforeCommitting)
			{
				if (!VerifyGameObject(askPanel, out Button button))
					throw new InvalidCastException($"'{askPanel.name}' does not have a valid button named '{quitButtonKey}'");
				Instantiate(askPanel, transform).GetComponent<Button>().onClick.AddListener(() => { askBeforeCommitting = false; QuitButtonBehaviour(); });
				throw new NotImplementedException();
			}

			if (quitToScene)
				Application.Quit();
			else
				_ = SceneManager.InstanceOrDefault.ChangeScene(sceneName);
		}
	}
}