using System;
using Mayfly.Akinator.Model;

namespace Mayfly.Akinator.Utils
{
	internal static class AkiUrlBuilder
	{
		public static string NewGame(ApiKey apiKey, IAkinatorServer server, bool childMode)
		{
			string childSwitch = string.Empty;
			string questionFilter = string.Empty;
			
			if (childMode)
			{
				childSwitch = "true";
				questionFilter = "cat%3D1";
			}
			
			return $"https://en.akinator.com/new_session?callback=jQuery3410014644797238627216_{GetTime()}&urlApiWs={Uri.EscapeDataString(server.ServerUrl)}&player=website-desktop&&partner=1&uid_ext_session={apiKey.SessionUid}&frontaddr={apiKey.FrontAdress.UrlEncode()}&childMod={childSwitch}&constraint={Uri.EscapeDataString("ETAT<>'AV'")}&soft_constraint=&question_filter={questionFilter}&_={GetTime()}";
		}

		public static string MapHallOfFame(IAkinatorServer server)
		{
			return $"http://classement.akinator.com:18666//get_hall_of_fame.php?basel_id={server.BaseId}";
		}

		public static string Answer(AnswerRequest request, IAkinatorServer server)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return $"{server.ServerUrl}/answer?session={request.Session}&signature={request.Signature}&step={request.Step}&answer={(int)request.Choice}";
		}

		public static string UndoAnswer(string session, string signature, int step, IAkinatorServer server)
		{
			return $"{server.ServerUrl}/cancel_answer?session={session}&signature={signature}&step={step}&answer=-1";
		}
		
		public static string SearchCharacter(string search, string session, string signature, int step, IAkinatorServer server)
		{
			return $"{server.ServerUrl}/soundlike_search?session={session}&signature={signature}&step={step}&name={search.UrlEncode()}";
		}

		public static string GetGuessUrl(GuessRequest request, IAkinatorServer server)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			string url = $"{server.ServerUrl}/list?session={request.Session}&signature={request.Signature}&step={request.Step}";
			return url;
		}

		private static long GetTime()
		{
			DateTime st = new DateTime(1970, 1, 1);
			TimeSpan t = DateTime.Now.ToUniversalTime() - st;
			
			return (long)(t.TotalMilliseconds + 0.5);
		}
	}
}
