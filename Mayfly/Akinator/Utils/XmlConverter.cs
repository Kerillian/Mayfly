using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Mayfly.Akinator.Utils
{
	public static class XmlConverter
	{
		public static T ToClass<T>(string data)
		{
			T response = default(T);

			if (!string.IsNullOrEmpty(data))
			{
				XmlReaderSettings settings = new XmlReaderSettings()
				{
					IgnoreWhitespace = true
				};

				XmlSerializer serializer = CachedXmlSerializerFactory.Create(typeof(T));
				XmlReader reader = XmlReader.Create(new StringReader(data), settings);
				
				response = (T)Convert.ChangeType(serializer.Deserialize(reader), typeof(T));
			}
			
			return response;
		}
	}
}