namespace HideousDestructor.DataPersistence
{
	using B1NARY;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Serialization;
	using SysXmlSerializer = System.Xml.Serialization.XmlSerializer;
	using UnityEngine;

	public static class XmlSerializer
	{
		private static readonly Dictionary<string, SysXmlSerializer> serializers =
			new Dictionary<string, SysXmlSerializer>();
		public static SysXmlSerializer GetOrNewSerializer(Type type)
		{
			if (serializers.TryGetValue(type.Name, out SysXmlSerializer value))
				return value;
			return serializers[type.Name] = new XmlSerializerFactory().CreateSerializer(type);
		}
		public static T Deserialize<T>(Stream stream)
		{
			T value = (T)GetOrNewSerializer(typeof(T)).Deserialize(stream);
			return value;
		}
		public static T Deserialize<T>(FileInfo fileInfo)
		{
			using (var readStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
				return Deserialize<T>(readStream);
		}
		public static void Serialize<T>(FileInfo fileInfo, T value)
		{
			using (var createStream = new FileStream(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write))
				Serialize(createStream, value);
		}
		public static void Serialize<T>(FileInfo fileInfo, T value, out FileStream fileStream)
		{
			fileStream = new FileStream(fileInfo.FullName, FileMode.CreateNew, FileAccess.Write);
			Serialize(fileStream, value);
			fileStream.Position = 0;
		}
		public static void Serialize<T>(Stream stream, T value)
		{

			GetOrNewSerializer(typeof(T)).Serialize(XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "\t" }), value);
		}

		private const string typeAttributeName = "type";
		public static MemoryStream SerializeDictionary(in string rootElementName, IEnumerable<KeyValuePair<string, object>> pairs)
		{
			var stream = new MemoryStream();
			var writer = XmlWriter.Create(stream, new XmlWriterSettings());
			writer.WriteStartElement(rootElementName.Trim().Replace(' ', '_'));
			using (var enumerator = pairs.GetEnumerator())
				while (enumerator.MoveNext())
				{
					string line = AddElement(enumerator.Current.Key, enumerator.Current.Value);
					writer.WriteRaw(line);
				}
			writer.WriteEndElement();
			writer.Flush();
			stream.Position = 0;

			// Doing it again to have proper formatting
			var document = new XmlDocument();
			document.Load(stream);
			stream.Dispose();
			stream = new MemoryStream();
			writer = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });
			document.WriteTo(writer);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
		internal static string AddElement(string name, object value)
		{
			var stream = new MemoryStream();
			Type valueType = value.GetType();
			XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { OmitXmlDeclaration = true });
			new SysXmlSerializer(valueType).Serialize(xmlWriter, value);
			xmlWriter.Flush();
			stream.Position = 0;
			Debug.Log(new StreamReader(stream).ReadToEnd());
			stream.Position = 0;
			var document = new XmlDocument();
			document.Load(stream);
			XmlNode targetChild = document.FirstChild;
			string innerXml = targetChild.InnerXml;
			XmlElement newElement = document.CreateElement(name);
			document.RemoveChild(targetChild);
			document.AppendChild(newElement);
			newElement.InnerXml = innerXml;
			XmlAttribute xmlAttribute = document.CreateAttribute(typeAttributeName);
			xmlAttribute.Value = valueType.FullName;
			newElement.Attributes.Append(xmlAttribute);
			stream.Dispose();
			stream = new MemoryStream();
			document.Save(stream);
			stream.Position = 0;
			string output = new StreamReader(stream).ReadToEnd();
			stream.Dispose();
			return output;
		}
		internal static Type ByName(string name)
		{
			using (var enumerator = AppDomain.CurrentDomain.GetAssemblies().Reverse().GetEnumerator())
				while (enumerator.MoveNext())
				{
					Type type = enumerator.Current.GetType(name);
					if (type != null)
						return type;
				}
			return typeof(object);
		}
		public static Dictionary<string, object> DeserializeDictionary(Stream stream)
		{
			var document = new XmlDocument();
			document.Load(stream);
			XmlNodeList nodeList = document.LastChild.ChildNodes;
			var output = new Dictionary<string, object>(nodeList.Count);
			for (int i = 0; i < nodeList.Count; i++)
			{
				XmlNode currentNode = nodeList.Item(i);
				string variableName = currentNode.Name;
				Type type = ByName(currentNode.Attributes.GetNamedItem(typeAttributeName).Value);
				var subStream = new MemoryStream();
				var writer = XmlWriter.Create(subStream);
				// only fix for primitive data types is just hard-coded exceptions
				string localName;
				switch (type.FullName)
				{
					case "System.String": localName = "string"; break;
					case "System.Single": localName = "float"; break;
					case "System.Int32": localName = "int"; break;
					case "System.Boolean": localName = "boolean"; break;
					default: localName = type.FullName; break;
				};
				writer.WriteStartElement(localName);
				writer.WriteRaw(currentNode.InnerXml);
				writer.WriteEndElement();
				writer.Flush();
				subStream.Position = 0;

				Console.WriteLine(new StreamReader(subStream).ReadToEnd());
				subStream.Position = 0;

				object value = GetOrNewSerializer(type).Deserialize(subStream);
				output[variableName] = value;
				subStream.Dispose();
			}
			return output;
		}
	}
}