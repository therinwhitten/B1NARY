namespace B1NARY
{
	using System.Linq;
	using System.Text;
	using UnityEngine;

	[RequireComponent(typeof(Camera))]
	public sealed class MainCameraHandler : MonoBehaviour
	{
		private Camera m_Camera;
		private void Awake()
		{
			m_Camera = GetComponent<Camera>();
		}
		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			SceneCheck();
		}
		private void SceneCheck()
		{
			Camera[] cameras = FindObjectsOfType<Camera>().Where(cam => cam != m_Camera).ToArray();
			if (cameras.Any())
			{
				var errorBuilder = new StringBuilder("There were more than 1 camera in scene, disposing existing cameras..");
				for (int i = 0; i < cameras.Length; i++)
				{
					errorBuilder.Append($"\n{cameras[i]}");
					Destroy(cameras[i]);
				}
				Debug.LogError(errorBuilder);
			}
			TransitionHandler.SwitchedScenes += str => SceneCheck();
		}
	}
}