using System.Globalization;

namespace Mayfly.UrlBuilders
{
	public enum JikanAnimeType
	{
		Tv,
		Movie,
		Ova,
		Special,
		Ona,
		Music
	}

	public enum JikanRating
	{
		G,
		Pg,
		Pg13,
		R17,
		R,
		Rx
	}

	public enum JikanSort
	{
		Descending,
		Ascending
	}
	
	public class JikanSearchBuilder : BaseUrlBuilder
	{
		private const string Endpoint = "https://api.jikan.moe/v4/anime";
		
		public JikanSearchBuilder() : base(Endpoint)
		{ }

		public JikanSearchBuilder WithPage(int page)
		{
			AppendParameter("page", page);
			return this;
		}

		public JikanSearchBuilder WithLimit(int limit)
		{
			AppendParameter("limit", limit);
			return this;
		}

		public JikanSearchBuilder WithQuery(string q)
		{
			AppendParameter("q", Uri.EscapeDataString(q));
			return this;
		}

		public JikanSearchBuilder WithType(JikanAnimeType type)
		{
			AppendParameter("type", GetEnumString(type).ToLower());
			return this;
		}

		public JikanSearchBuilder WithRating(JikanRating rating)
		{
			AppendParameter("rating", rating);
			return this;
		}

		public JikanSearchBuilder WithSfw(bool sfw)
		{
			AppendParameter("sfw", sfw);
			return this;
		}

		public JikanSearchBuilder WithSort(JikanSort sort)
		{
			AppendParameter("sort", sort);
			return this;
		}

		public JikanSearchBuilder WithLetter(char letter)
		{
			AppendParameter("letter", char.ToString(letter));
			return this;
		}

		#region Genres
		
		public JikanSearchBuilder WithGenres(string genres)
		{
			
			AppendParameter("genres", string.Join(',', genres.Split(',').Select(int.Parse)));
			return this;
		}

		public JikanSearchBuilder WithGenres(params int[] genres)
		{
			AppendParameter("genres", string.Join(',', genres));
			return this;
		}
		
		#endregion

		#region Geners Exclude
		
		public JikanSearchBuilder WithExclude(string genres)
		{
			AppendParameter("genres_exclude", genres);
			return this;
		}

		public JikanSearchBuilder WithExclude(params int[] genres)
		{
			AppendParameter("genres_exclude", string.Join(',', genres));
			return this;
		}
		
		#endregion

		#region Producers

		public JikanSearchBuilder WithProducers(string producers)
		{
			AppendParameter("producers", producers);
			return this;
		}

		public JikanSearchBuilder WithProducers(params int[] producers)
		{
			AppendParameter("producers", string.Join(',', producers));
			return this;
		}

		#endregion
	}
}