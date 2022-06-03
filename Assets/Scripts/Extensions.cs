using System;


public static class Extensions
{
	public static bool TryInvoke(this Action action)
	{
		try { action.Invoke(); }
		catch { return false; }
		return true;
	}
	public static bool TryInvoke<TOut>(this Func<TOut> func, out TOut @out)
	{
		try { @out = func.Invoke(); }
		catch
		{
			@out = default;
			return false;
		}
		return true;
	}
}