using System.Collections.Generic;

public sealed class DebuggerPreferences : 
	Dictionary<DebuggerPreferences.DataType, List<(string name, object @default)>>
{
	public enum DataType
	{
		Bool,
		Int,
		Float,
		String,
		StringPopup,
	}
}