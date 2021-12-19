using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "LANGUAGE")]
	public class ServerSearchResultLanguage
	{
		[XmlElement(ElementName = "LANG_ID")]
		public string LANG_ID { get; set; }
	}
}
