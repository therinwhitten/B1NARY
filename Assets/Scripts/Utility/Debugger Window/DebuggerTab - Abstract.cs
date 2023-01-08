#if UNITY_EDITOR
namespace B1NARY.Editor.Debugger
{
	using System.Linq;
	using System.Reflection;
	using System.Collections.Generic;
	using UnityEditor;
	using System;
	using UnityEngine;
	using System.Collections.ObjectModel;

	public abstract class DebuggerTab : IComparable<DebuggerTab>
	{
		public static IReadOnlyList<DebuggerTab> AllTabs => m_allTabs.Value;
		private static readonly Lazy<DebuggerTab[]> m_allTabs = new Lazy<DebuggerTab[]>(() =>
		{
			return (
				from type in typeof(DebuggerTab).Assembly.GetTypes()
				where type.IsSubclassOf(typeof(DebuggerTab)) && !type.IsAbstract && type.IsClass
				let tab = (DebuggerTab)Activator.CreateInstance(type)
				orderby tab.Order descending
				select tab
				).ToArray();
		});
		public static DebuggerPreferences Preferences => m_preferences.Value;
		private static readonly Lazy<DebuggerPreferences> m_preferences = new Lazy<DebuggerPreferences>(() =>
		{
			var enumerator = AllTabs.SelectMany(tab => tab.DebuggerPreferences).GetEnumerator();
			var prefs = new DebuggerPreferences();
			while (enumerator.MoveNext())
				prefs.Add(enumerator.Current.Key, enumerator.Current.Value);
			return prefs;
		});

		public abstract bool ConstantlyRepaint { get; }
		public abstract GUIContent Name { get; }
		public abstract void DisplayTab();
		public virtual int Order => 0;
		public virtual bool ShowInDebugger => true;

		// Memory for normal files
		public virtual DebuggerPreferences DebuggerPreferences { get; } = null;

		public int CompareTo(DebuggerTab other) => other.Order.CompareTo(Order);
		public override string ToString() => Name + " (DebuggerTab)";
	}
}
#endif