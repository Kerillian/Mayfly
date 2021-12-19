using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "CAPS_DISABLED")]
	public class ServerSearchResultCapsDisabled
	{
		[XmlElement(ElementName = "CAP")]
		public List<string> CAP { get; set; }
	}
}