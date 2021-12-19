using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[Serializable, XmlRoot("AWARDS")]
	public class AwardCollection
	{
		[XmlElement(ElementName = "AWARD")]
		public List<Award> Award { get; set; }
	}
}