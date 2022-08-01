namespace B1NARY.Editor.Debugger
{
	using System.Linq;
	using System.Reflection;
	using System.Collections.Generic;
	using UnityEditor;
	using System;

	public abstract class DebuggerTab : IComparable<DebuggerTab>
	{

		static DebuggerTab()
		{
			Tabs = (
				from type in typeof(DebuggerTab).Assembly.GetTypes()
				where type.IsSubclassOf(typeof(DebuggerTab)) && !type.IsAbstract && type.IsClass
				let tab = (DebuggerTab)Activator.CreateInstance(type)
				select tab
				).ToArray();
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
		public static DebuggerTab[] ShownTabs =>
			(from tab in Tabs
			 let type = tab.GetType()
			 where EditorPrefs.GetBool($"B1NARY Tab: {tab.Name}", tab.ShowInDebugger)
			 select tab).ToArray();
		public static (string name, DebuggerPreferences data)[] Data { get; private set; }


		public abstract string Name { get; }
		public abstract void DisplayTab();
		public virtual int Order => 0;
		public virtual bool ShowInDebugger => true;

		// Memory for normal files
		public virtual DebuggerPreferences DebuggerPreferences { get; } = null;

		public int CompareTo(DebuggerTab other) => other.Order.CompareTo(Order);
		public override string ToString() => Name + " (DebuggerTab)";
	}
}