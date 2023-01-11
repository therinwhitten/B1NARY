namespace HideousDestructor.DataPersistence
{
	using B1NARY;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Serialization;
	using SysXmlSerializer = System.Xml.Serialization.XmlSerializer;

	public static class XmlSerializer
	{
		private static readonly Dictionary<string, SysXmlSerializer> serializers =
			new Dictionary<string, SysXmlSerializer>();
		private static SysXmlSerializer GetOrNewSerializer(Type type)
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
		public static void Serialize<T>(Stream stream, T value)
		{
			GetOrNewSerializer(typeof(T)).Serialize(stream, value);
		}
	}
}