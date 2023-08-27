namespace B1NARY.UI.Colors
{
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using HDConsole.IO;
	using OVSXmlSerializer;

	public class IndexFile : IXmlSerializable
	{
		public static OSFile IndexPath => ColorFormat.RootPath.GetFile("PlayerThemeIndex.xml");
		private static XmlSerializer<IndexFile> indexerFormatter = new();
		public static IndexFile LoadNew()
		{
			if (IndexPath.Exists)
				using (FileStream stream = IndexPath.OpenRead())
					return indexerFormatter.Deserialize(stream);
			else
				return new IndexFile();
		}
		public List<string> files = new();

		bool IXmlSerializable.ShouldWrite => true;
		void IXmlSerializable.Read(XmlNode value)
		{
			files = new List<string>(value.ChildNodes.Count);
			for (int i = 0; i < files.Capacity; i++)
				files.Add(value.ChildNodes[i].InnerText);
		}
		void IXmlSerializable.Write(XmlDocument document, XmlNode node)
		{
			for (int i = 0; i < files.Count; i++)
			{
				XmlElement element = document.CreateElement("File");
				element.InnerText = files[i];
				node.AppendChild(element);
			}
		}
		public void Save()
		{
			using FileStream stream = IndexPath.Create();
			indexerFormatter.Serialize(stream, this);
		}
	}
}