namespace B1NARY.Scripting
{
	using System;

	/// <summary>
	/// This is used to prevent the <see cref="ScriptNode.Perform(bool)"/> from
	/// continuing after invoking the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ForcePauseAttribute : Attribute
	{
		public ForcePauseAttribute() : base()
		{

		}
	}
}