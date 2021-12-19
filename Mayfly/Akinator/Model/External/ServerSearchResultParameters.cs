using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "PARAMETERS")]
	public class ServerSearchResultParameters
	{
		[XmlElement(ElementName = "INSTANCE")]
		public List<ServerSearchResultInstance> INSTANCE { get; set; }
	}
}