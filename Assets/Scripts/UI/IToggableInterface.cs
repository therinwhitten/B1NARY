namespace B1NARY.UI
{
	using UnityEngine.InputSystem;

	public interface ITogglableInterface
	{

		string ActionName { get; }
		void TogglePlayerOpen(InputAction.CallbackContext context);
	}
}