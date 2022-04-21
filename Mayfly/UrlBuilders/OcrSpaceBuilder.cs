namespace Mayfly.UrlBuilders
{
	public enum OcrSpaceLanguage
	{
		Arabic,
		Bulgarian,
		ChineseSimplified,
		ChineseTraditional,
		Croatian,
		Czech,
		Danish,
		Dutch,
		English,
		Finnish,
		French,
		German,
		Greek,
		Hungarian,
		Korean,
		Italian,
		Japanese,
		Polish,
		Portuguese,
		Russian,
		Slovenian,
		Spanish,
		Swedish,
		Turkish
	}

	public enum OcrSpaceFiletype
	{
		PDF,
		GIF,
		PNG,
		JPG,
		TIF,
		BMP
	}
	
	public enum OcrSpaceEngine
	{
		Engine1 = 1,
		Engine2 = 2,
		Engine3 = 3
	}
	
	public class OcrSpaceBuilder : BaseUrlBuilder
	{
		private const string Endpoint = "https://api.ocr.space/parse/imageurl";

		public OcrSpaceBuilder(string key) : base(Endpoint)
		{
			AppendParameter("apikey", key);
		}

		private static string ToLanguageCode(OcrSpaceLanguage language)
		{
			return language switch
			{
				OcrSpaceLanguage.Arabic             => "ara",
				OcrSpaceLanguage.Bulgarian          => "bul",
				OcrSpaceLanguage.ChineseSimplified  => "chs",
				OcrSpaceLanguage.ChineseTraditional => "cht",
				OcrSpaceLanguage.Croatian           => "hrv",
				OcrSpaceLanguage.Czech              => "cze",
				OcrSpaceLanguage.Danish             => "dan",
				OcrSpaceLanguage.Dutch              => "dut",
				OcrSpaceLanguage.English            => "eng",
				OcrSpaceLanguage.Finnish            => "fin",
				OcrSpaceLanguage.French             => "fre",
				OcrSpaceLanguage.German             => "ger",
				OcrSpaceLanguage.Greek              => "gre",
				OcrSpaceLanguage.Hungarian          => "hun",
				OcrSpaceLanguage.Korean             => "kor",
				OcrSpaceLanguage.Italian            => "ita",
				OcrSpaceLanguage.Japanese           => "jpn",
				OcrSpaceLanguage.Polish             => "pol",
				OcrSpaceLanguage.Portuguese         => "por",
				OcrSpaceLanguage.Russian            => "rus",
				OcrSpaceLanguage.Slovenian          => "slv",
				OcrSpaceLanguage.Spanish            => "spa",
				OcrSpaceLanguage.Swedish            => "swe",
				OcrSpaceLanguage.Turkish            => "tur",
				_                                   => "eng"
			};
		}

		public OcrSpaceBuilder WithUrl(string url)
		{
			AppendParameter("url", Uri.EscapeDataString(url));
			return this;
		}

		public OcrSpaceBuilder WithLanguage(OcrSpaceLanguage language)
		{
			AppendParameter("language", ToLanguageCode(language));
			return this;
		}
		
		public OcrSpaceBuilder WithScale(bool scale)
		{
			AppendParameter("scale", scale);
			return this;
		}

		public OcrSpaceBuilder WithOcrEngine(OcrSpaceEngine engine)
		{
			AppendParameter("OCREngine", (int)engine);
			return this;
		}
		
		public OcrSpaceBuilder WithIsTable(bool isTable)
		{
			AppendParameter("isTable", isTable);
			return this;
		}
		
		public OcrSpaceBuilder WithOverlayRequired(bool overlayRequired)
		{
			AppendParameter("isOverlayRequired", overlayRequired);
			return this;
		}
		
		public OcrSpaceBuilder WithFiletype(OcrSpaceFiletype type)
		{
			AppendParameter("filetype", GetEnumString(type));
			return this;
		}

		public OcrSpaceBuilder WithDetectOrientation(bool detect)
		{
			AppendParameter("detectOrientation", detect);
			return this;
		}

		public OcrSpaceBuilder WithIsCreateSearchablePdf(bool isCreateSearchablePdf)
		{
			AppendParameter("isCreateSearchablePdf", isCreateSearchablePdf);
			return this;
		}
		
		public OcrSpaceBuilder WithIsSearchablePdfHideTextLayer(bool isSearchablePdfHideTextLayer)
		{
			AppendParameter("isSearchablePdfHideTextLayer", isSearchablePdfHideTextLayer);
			return this;
		}
	}
}