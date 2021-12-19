using System;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class DanbooruPost
	{
		[JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("uploader_id")]
        public int UploaderId { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("last_comment_bumped_at")]
        public DateTime? LastCommentBumpedAt { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("image_width")]
        public int ImageWidth { get; set; }

        [JsonProperty("image_height")]
        public int ImageHeight { get; set; }

        [JsonProperty("tag_string")]
        public string TagString { get; set; }

        [JsonProperty("is_note_locked")]
        public bool IsNoteLocked { get; set; }

        [JsonProperty("fav_count")]
        public int FavCount { get; set; }

        [JsonProperty("file_ext")]
        public string FileExt { get; set; }

        [JsonProperty("last_noted_at")]
        public DateTime? LastNotedAt { get; set; }

        [JsonProperty("is_rating_locked")]
        public bool IsRatingLocked { get; set; }

        [JsonProperty("parent_id")]
        public int? ParentId { get; set; }

        [JsonProperty("has_children")]
        public bool HasChildren { get; set; }

        [JsonProperty("approver_id")]
        public int? ApproverId { get; set; }

        [JsonProperty("tag_count_general")]
        public int TagCountGeneral { get; set; }

        [JsonProperty("tag_count_artist")]
        public int TagCountArtist { get; set; }

        [JsonProperty("tag_count_character")]
        public int TagCountCharacter { get; set; }

        [JsonProperty("tag_count_copyright")]
        public int TagCountCopyright { get; set; }

        [JsonProperty("file_size")]
        public int FileSize { get; set; }

        [JsonProperty("is_status_locked")]
        public bool IsStatusLocked { get; set; }

        [JsonProperty("up_score")]
        public int UpScore { get; set; }

        [JsonProperty("down_score")]
        public int DownScore { get; set; }

        [JsonProperty("is_pending")]
        public bool IsPending { get; set; }

        [JsonProperty("is_flagged")]
        public bool IsFlagged { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("tag_count")]
        public int TagCount { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("is_banned")]
        public bool IsBanned { get; set; }

        [JsonProperty("pixiv_id")]
        public int PixivId { get; set; }

        [JsonProperty("last_commented_at")]
        public DateTime? LastCommentedAt { get; set; }

        [JsonProperty("has_active_children")]
        public bool HasActiveChildren { get; set; }

        [JsonProperty("bit_flags")]
        public int BitFlags { get; set; }

        [JsonProperty("tag_count_meta")]
        public int TagCountMeta { get; set; }

        [JsonProperty("has_large")]
        public bool HasLarge { get; set; }

        [JsonProperty("has_visible_children")]
        public bool HasVisibleChildren { get; set; }

        [JsonProperty("tag_string_general")]
        public string TagStringGeneral { get; set; }

        [JsonProperty("tag_string_character")]
        public string TagStringCharacter { get; set; }

        [JsonProperty("tag_string_copyright")]
        public string TagStringCopyright { get; set; }

        [JsonProperty("tag_string_artist")]
        public string TagStringArtist { get; set; }

        [JsonProperty("tag_string_meta")]
        public string TagStringMeta { get; set; }

        [JsonProperty("file_url")]
        public string FileUrl { get; set; }

        [JsonProperty("large_file_url")]
        public string LargeFileUrl { get; set; }

        [JsonProperty("preview_file_url")]
        public string PreviewFileUrl { get; set; }

        [JsonIgnore]
        public string[] TagArray => TagString.Split(' ');
	}
}