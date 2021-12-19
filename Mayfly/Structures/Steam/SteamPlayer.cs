using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SteamPlayer
	{
		[JsonProperty("steamid")]
		public string SteamId { get; set; }
		
		[JsonProperty("personaname")]
		public string PersonaName { get; set; }
		
		[JsonProperty("profileurl")]
		public string ProfileUrl { get; set; }
		
		[JsonProperty("avatar")]
		public string Avatar { get; set; }
		
		[JsonProperty("avatarmedium")]
		public string AvatarMedium { get; set; }
		
		[JsonProperty("avatarfull")]
		public string AvatarFull { get; set; }

		[JsonIgnore]
		public bool IsVanity => this.ProfileUrl.Contains("/id/");
	}
}