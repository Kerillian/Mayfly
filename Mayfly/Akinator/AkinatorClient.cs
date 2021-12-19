using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Mayfly.Akinator.Enumerations;
using Mayfly.Akinator.Model;
using Mayfly.Akinator.Model.External;
using Mayfly.Akinator.Utils;

namespace Mayfly.Akinator
{
	public class AkinatorClient : IAkinatorClient
	{
		private readonly Regex m_regexSession = new Regex("var uid_ext_session = '(.*)'\\;\\n.*var frontaddr = '(.*)'\\;");
		private readonly Regex m_regexStartGameResult = new Regex(@"^jQuery3410014644797238627216_\d+\((.+)\)$");
		private readonly AkiWebClient m_webClient;
		private readonly IAkinatorServer m_server;
		private readonly bool m_childMode;
		private string m_session;
		private string m_signature;
		private int m_step;
		private int m_lastGuessStep;

		public AkinatorClient(IAkinatorServer server, AkinatorUserSession existingSession = null, bool childMode = false)
		{
			m_webClient = new AkiWebClient();
			m_server = server;
			m_childMode = childMode;
			Attach(existingSession);
		}

		public async Task<AkinatorQuestion> StartNewGame(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ApiKey apiKey = await GetSession(cancellationToken).ConfigureAwait(false);
			string url = AkiUrlBuilder.NewGame(apiKey, m_server, m_childMode);
			HttpResponseMessage response = await m_webClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			Match match = m_regexStartGameResult.Match(content);

			if (!match.Success && match.Groups.Count != 2)
			{
				throw new InvalidCastException($"Invalid result received from Akinator. Result was {response}");
			}

			BaseResponse<NewGameParameters> result = JsonConvert.DeserializeObject<BaseResponse<NewGameParameters>>(match.Groups[1].Value, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			m_session = result.Parameters.Identification.Session;
			m_signature = result.Parameters.Identification.Signature;
			m_step = result.Parameters.StepInformation.Step;
			CurrentQuestion = ToAkinatorQuestion(result.Parameters.StepInformation);
			return ToAkinatorQuestion(result.Parameters.StepInformation);
		}

		public async Task<AkinatorQuestion> Answer(AnswerOptions answer, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string url = AkiUrlBuilder.Answer(BuildAnswerRequest(answer), m_server);
			HttpResponseMessage response = await m_webClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			BaseResponse<Question> result = JsonConvert.DeserializeObject<BaseResponse<Question>>(content, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			m_step = result.Parameters.Step;
			CurrentQuestion = ToAkinatorQuestion(result.Parameters);
			return ToAkinatorQuestion(result.Parameters);
		}

		public async Task<AkinatorQuestion> UndoAnswer(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (m_step == 0)
			{
				return null;
			}

			string url = AkiUrlBuilder.UndoAnswer(m_session, m_signature, m_step, m_server);
			HttpResponseMessage response = await m_webClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			BaseResponse<Question> result = JsonConvert.DeserializeObject<BaseResponse<Question>>(content, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			m_step = result.Parameters.Step;
			CurrentQuestion = ToAkinatorQuestion(result.Parameters);
			return ToAkinatorQuestion(result.Parameters);
		}

		public async Task<AkinatorGuess[]> SearchCharacter(string search, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string url = AkiUrlBuilder.SearchCharacter(search, m_session, m_signature, m_step, m_server);
			HttpResponseMessage response = await m_webClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			BaseResponse<Characters> result = JsonConvert.DeserializeObject<BaseResponse<Characters>>(content, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			return result.Parameters.AllCharacters.Select(p => new AkinatorGuess(p.Name, p.Description)
			{
				ID = p.IdBase,
				PhotoPath = p.PhotoPath,
			}).ToArray();
		}

		public AkinatorQuestion CurrentQuestion { get; private set; }

		public async Task<AkinatorHallOfFameEntries[]> GetHallOfFame(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string hallOfFameRequestUrl = AkiUrlBuilder.MapHallOfFame(m_server);
			HttpResponseMessage response = await m_webClient.GetAsync(hallOfFameRequestUrl, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			HallOfFame data = XmlConverter.ToClass<HallOfFame>(content);
			return ToHallOfFameEntry(data.Awards.Award);
		}

		public async Task<AkinatorGuess[]> GetGuess(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string url = AkiUrlBuilder.GetGuessUrl(BuildGuessRequest(), m_server);
			HttpResponseMessage response = await m_webClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			BaseResponse<Guess> result = JsonConvert.DeserializeObject<BaseResponse<Guess>>(content, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			m_lastGuessStep = m_step;
			
			return result.Parameters.Characters.Select(p => new AkinatorGuess(p.Name, p.Description)
			{
				ID = p.Id,
				PhotoPath = p.PhotoPath,
				Probability = p.Probability
			}).ToArray();
		}

		public bool GuessIsDue(Platform platform = Platform.Android) => GuessDueChecker.GuessIsDue(CurrentQuestion, m_lastGuessStep, platform);

		private async Task<ApiKey> GetSession(CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await m_webClient.GetAsync("https://en.akinator.com/game", cancellationToken).ConfigureAwait(false);

			if (response?.StatusCode != HttpStatusCode.OK)
			{
				throw new InvalidOperationException("Cannot connect to Akinator.com");
			}

			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			Match match = m_regexSession.Match(content);

			if (!match.Success)
			{
				throw new InvalidOperationException("Cannot retrieve the Api-Key from Akinator.com");
			}

			ApiKey apiKey = new ApiKey()
			{
				SessionUid = match.Groups[1].Value,
				FrontAdress = match.Groups[2].Value
			};

			return apiKey;
		}

		public AkinatorUserSession GetUserSession()
		{
			return new AkinatorUserSession(m_session, m_signature, m_step, m_lastGuessStep);
		}

		private static AkinatorQuestion ToAkinatorQuestion(Question question)
		{
			return new AkinatorQuestion(question.Text, question.Progression, question.Step);
		}

		private static AkinatorHallOfFameEntries[] ToHallOfFameEntry(List<Award> awardsAward)
		{
			return awardsAward.Select(p => new AkinatorHallOfFameEntries(p.AwardId,p.CharacterName,p.Description,p.Type,p.WinnerName,p.Delai,p.Pos)).ToArray();
		}

		private GuessRequest BuildGuessRequest()
		{
			return new GuessRequest(m_step, m_session, m_signature);
		}

		private AnswerRequest BuildAnswerRequest(AnswerOptions choice)
		{
			return new AnswerRequest(choice, m_step, m_session, m_signature);
		}

		private void Attach(AkinatorUserSession existingSession)
		{
			if (existingSession != null)
			{
				m_step = existingSession.Step;
				m_lastGuessStep = existingSession.LastGuessStep;
				m_session = existingSession.Session;
				m_signature = existingSession.Signature;
			}
		}

		public void Dispose()
		{
			m_webClient?.Dispose();
		}
	}
}