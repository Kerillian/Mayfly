using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "RESULT")]
	public class ServerSearchResult
	{
		[XmlElement(ElementName = "COMPLETION")]
		public string COMPLETION { get; set; }

		[XmlElement(ElementName = "CODE_PAYS")]
		public string CODE_PAYS { get; set; }
		
		[XmlElement(ElementName = "PARAMETERS")]
		public ServerSearchResultParameters PARAMETERS { get; set; }
	}
}