using System;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class ImgurPost
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("hash")]
		public string Hash { get; set; }

		[JsonProperty("author")]
		public string Author { get; set; }

		[JsonProperty("account_id")]
		public object AccountId { get; set; }

		[JsonProperty("account_url")]
		public object AccountUrl { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("score")]
		public long Score { get; set; }

		[JsonProperty("size")]
		public long Size { get; set; }

		[JsonProperty("views")]
		public long Views { get; set; }

		[JsonProperty("is_album")]
		public bool IsAlbum { get; set; }

		[JsonProperty("album_cover")]
		public string AlbumCover { get; set; }

		[JsonProperty("album_cover_width")]
		public long AlbumCoverWidth { get; set; }

		[JsonProperty("album_cover_height")]
		public long AlbumCoverHeight { get; set; }

		[JsonProperty("mimetype")]
		public object Mimetype { get; set; }

		[JsonProperty("ext")]
		public string Ext { get; set; }

		[JsonProperty("width")]
		public long Width { get; set; }

		[JsonProperty("height")]
		public long Height { get; set; }

		[JsonProperty("animated")]
		public bool Animated { get; set; }

		[JsonProperty("looping")]
		public bool Looping { get; set; }

		[JsonProperty("reddit")]
		public string Reddit { get; set; }

		[JsonProperty("subreddit")]
		public string Subreddit { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("create_datetime")]
		public string CreateDatetime { get; set; }

		[JsonProperty("bandwidth")]
		public object Bandwidth { get; set; }

		[JsonProperty("timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[JsonProperty("section")]
		public string Section { get; set; }

		[JsonProperty("nsfw")]
		public bool Nsfw { get; set; }

		[JsonProperty("prefer_video")]
		public bool PreferVideo { get; set; }

		[JsonProperty("video_source")]
		public string VideoSource { get; set; }

		[JsonProperty("video_host")]
		public object VideoHost { get; set; }

		[JsonProperty("num_images")]
		public long NumImages { get; set; }

		[JsonProperty("in_gallery")]
		public bool InGallery { get; set; }

		[JsonProperty("favorited")]
		public bool Favorited { get; set; }

		[JsonProperty("adConfig")]
		public ImgurAdConfig AdConfig { get; set; }
	}
}