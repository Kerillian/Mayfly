using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "CANDIDATS")]
	public class ServerSearchResultCandidats
	{
		[XmlElement(ElementName = "URL")]
		public List<string> URL { get; set; }
	}
}