using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using System;

public abstract class DebuggerTab : IComparable<DebuggerTab>
{
	
	static DebuggerTab()
	{
		Tabs = typeof(DebuggerTab).Assembly.GetTypes()
			.Where(obj => obj.IsSubclassOf(typeof(DebuggerTab)) && !obj.IsAbstract && obj.IsClass)
			.Select(type => (DebuggerTab)Activator.CreateInstance(type)).ToArray();
		Array.Sort(Tabs);
		DefineData();
	}
	private static void DefineData()
	{
		Data = (
			from tab in Tabs
			where tab.DebuggerPreferences != null
			select (tab.Name, tab.DebuggerPreferences)).ToArray();
		/* This code turns all datatabs into a single merged DataPreference

		IEnumerable<DebuggerPreferences> dataTabs =
			Tabs.Select(tab => tab.DebuggerPreferences).Where(pref => pref != null);
		if (!dataTabs.Any())
			return;
		Data = new DebuggerPreferences();
		foreach (var dataTab in dataTabs)
		{
			IEnumerable<DebuggerPreferences.DataType> dataTypes = dataTab.Select(x => x.Key);
			foreach (var dataType in dataTypes)
			{
				if (Data.ContainsKey(dataType))
					Data.Add(dataType, new List<(string name, object @default)>());
				Data[dataType].AddRange(dataTab[dataType]);
			}

		}
		*/
	}
	public static DebuggerTab[] Tabs { get; private set; }
	public static (string name, DebuggerPreferences data)[] Data { get; private set; } 

	
	public abstract string Name { get; }
	public abstract void DisplayTab();
	public virtual int Order => 0;

	// Memory for normal files
	public virtual DebuggerPreferences DebuggerPreferences { get; } = null;

	public int CompareTo(DebuggerTab other) => other.Order.CompareTo(Order);
	public override string ToString() => Name + " (DebuggerTab)";
}