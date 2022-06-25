﻿using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GamePreferences
{
	[ExecuteAlways]
	static GamePreferences()
	{
		GetGameDataDocument();
	}

	// Data Management
	private static readonly string[] dataTypes = { "string", "int", "bool", "float" };
	private static XmlDocument gameDataDocument = null;
	private static Dictionary<string, string> stringSettings;
	private static Dictionary<string, bool> boolSettings;
	private static Dictionary<string, int> intSettings;
	private static Dictionary<string, float> floatSettings;
	private static bool HasBig4Data => stringSettings != null && boolSettings != null && intSettings != null && floatSettings != null;
	private static string FileXMLName => Application.streamingAssetsPath + "/B1NARY Game Prefs.xml";
	private const string RootFileName = "B1NARYGamePreferences";


	public static void ResetXMLFile()
	{
		gameDataDocument = null;
		GetGameDataDocument();
	}
	
	private static void GetGameDataDocument()
	{
		if (gameDataDocument != null && HasBig4Data)
			return;
		gameDataDocument = new XmlDocument();
		if (!File.Exists(FileXMLName))
		{
			var xmlWriterSettings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "  ",
			};
			using (XmlWriter writer = XmlWriter.Create(FileXMLName, xmlWriterSettings))
			{
				writer.WriteStartElement(RootFileName);
				foreach (var line in dataTypes)
				{
					writer.WriteStartElement(line);
					writer.WriteEndElement();
				}
				writer.Flush();
			}
			AssetDatabase.Refresh();
		}
		gameDataDocument.Load(FileXMLName);
		XmlNode dataNode = gameDataDocument.LastChild;
		foreach (XmlNode node in dataNode.ChildNodes)
		{
			var innerDataText = new List<(string itemName, string data)>();
			for (int i = 0; i < node.ChildNodes.Count; i++)
				innerDataText.Add((node.ChildNodes[i].Name, node.ChildNodes[i].InnerText));
			switch (node.Name)
			{
				case "string":
					stringSettings = innerDataText.ToDictionary(tuple => tuple.itemName, tuple => tuple.data);
					continue;
				case "int":
					intSettings = innerDataText.ToDictionary(tuple => tuple.itemName, tuple => int.Parse(tuple.data));
					continue;
				case "bool":
					boolSettings = innerDataText.ToDictionary(tuple => tuple.itemName, tuple => bool.Parse(tuple.data));
					continue;
				case "float":
					floatSettings = innerDataText.ToDictionary(tuple => tuple.itemName, tuple => float.Parse(tuple.data));
					continue;
				default:
					throw new KeyNotFoundException(node.Name);
			}
		}
	}
	// This specifically saves data long-term, use the other methods to do that.
	private static void SaveData(int type, string name, string input)
	{
		bool valueExists = gameDataDocument.SelectSingleNode($"//{RootFileName}//{dataTypes[type]}//{name}") != null;
		if (string.IsNullOrEmpty(input))
		{
			if (!valueExists)
				return;
			XmlNode deletingChild = gameDataDocument.SelectSingleNode($"//{RootFileName}//{dataTypes[type]}//{name}");
			gameDataDocument.SelectSingleNode($"//{RootFileName}//{dataTypes[type]}").RemoveChild(deletingChild);
		}
		else
		{
			if (valueExists)
				((XmlElement)gameDataDocument.SelectSingleNode($"//{RootFileName}//{dataTypes[type]}//{name}")).InnerText = input;
			else
			{
				XmlElement element = gameDataDocument.CreateElement(name);
				element.InnerText = input;
				gameDataDocument.LastChild.ChildNodes[type].AppendChild(element);
			}

		}
		gameDataDocument.Save(FileXMLName);
	}


	public static void DeleteAllKeys()
	{
		if (File.Exists(FileXMLName))
		{
			File.Delete(FileXMLName);
			File.Delete(FileXMLName + ".meta");
		}
		ResetXMLFile();
	}

	public static string GetString(string name, string @default = default)
	{
		if (stringSettings.ContainsKey(name))
			return stringSettings[name];
		return @default;
	}
	public static void SetString(string name, string input)
	{
		if (stringSettings.ContainsKey(name))
			stringSettings[name] = input;
		else
			stringSettings.Add(name, input);
		SaveData(0, name, input);
	}
	public static void DeleteString(string name)
	{
		SaveData(0, name, string.Empty);
		stringSettings.Remove(name);
	}

	public static bool GetBool(string name, bool @default = default)
	{
		if (boolSettings.ContainsKey(name))
			return boolSettings[name];
		return @default;
	}
	public static void SetBool(string name, bool input)
	{
		if (boolSettings.ContainsKey(name))
			boolSettings[name] = input;
		else
			boolSettings.Add(name, input);
		SaveData(2, name, input.ToString());
	}
	public static void DeleteBool(string name)
	{
		SaveData(2, name, string.Empty);
		boolSettings.Remove(name);
	}

	public static int GetInt(string name, int @default = default)
	{
		if (intSettings.ContainsKey(name))
			return intSettings[name];
		return @default;
	}
	public static void SetInt(string name, int input)
	{
		if (intSettings.ContainsKey(name))
			intSettings[name] = input;
		else
			intSettings.Add(name, input);
		SaveData(1, name, input.ToString());
	}
	public static void DeleteInt(string name)
	{
		SaveData(1, name, string.Empty);
		intSettings.Remove(name);
	}

	public static float GetFloat(string name, float @default = default)
	{
		if (floatSettings.ContainsKey(name))
			return floatSettings[name];
		return @default;
	}
	public static void SetFloat(string name, float input)
	{
		if (floatSettings.ContainsKey(name))
			floatSettings[name] = input;
		else
			floatSettings.Add(name, input);
		SaveData(3, name, input.ToString());
	}
	public static void DeleteFloat(string name)
	{
		SaveData(3, name, string.Empty);
		floatSettings.Remove(name);
	}
}