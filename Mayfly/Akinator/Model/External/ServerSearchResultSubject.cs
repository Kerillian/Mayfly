using System.Xml.Serialization;

namespace Mayfly.Akinator.Model.External
{
	[XmlRoot(ElementName = "SUBJECT")]
	public class ServerSearchResultSubject
	{
		[XmlElement(ElementName = "SUBJ_ID")]
		public string SUBJ_ID { get; set; }

		[XmlElement(ElementName = "SUBJ_LABEL")]
		public string SUBJ_LABEL { get; set; }

		[XmlElement(ElementName = "SUBJ_PRICE")]
		public string SUBJ_PRICE { get; set; }

		[XmlElement(ElementName = "SUBJ_PICTURE_URL")]
		public string SUBJ_PICTURE_URL { get; set; }
		
		[XmlElement(ElementName = "SUBJ_ICON_URL")]
		public string SUBJ_ICON_URL { get; set; }
	}
}