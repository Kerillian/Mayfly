using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Interactions;
using Mayfly.Extensions;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class StringModule : MayflyModule
	{
		private static readonly Dictionary<char, string> FancyDic = new Dictionary<char, string>()
		{
			{'a', "🇦"}, {'b', "🇧"}, {'c', "🇨"}, {'d', "🇩"}, {'e', "🇪"}, {'f', "🇫"},
			{'g', "🇬"}, {'h', "🇭"}, {'i', "🇮"}, {'j', "🇯"}, {'k', "🇰"}, {'l', "🇱"},
			{'m', "🇲"}, {'n', "🇳"}, {'o', "🇴"}, {'p', "🇵"}, {'q', "🇶"}, {'r', "🇷"},
			{'s', "🇸"}, {'t', "🇹"}, {'u', "🇺"}, {'v', "🇻"}, {'w', "🇼"}, {'x', "🇽"},
			{'y', "🇾"}, {'z', "🇿"}, {'0', "0⃣"}, {'1', "1⃣"}, {'2', "2⃣"}, {'3', "3⃣"},
			{'4', "4⃣"}, {'5', "5⃣"}, {'6', "6⃣"}, {'7', "7⃣"}, {'8', "8⃣"}, {'9', "9⃣"},
			{'#', "#⃣"}, {'?', "❔"}, {'!', "❕"}, {' ', "	 "}
		};

		private static readonly char[] ZalgoChars = new char[111]
		{
			'\u0300', '\u0301', '\u0302', '\u0303', '\u0304', '\u0305', '\u0306', '\u0307',
			'\u0308', '\u0309', '\u030a', '\u030b', '\u030c', '\u030d', '\u030e', '\u030f',
			'\u0310', '\u0311', '\u0312', '\u0313', '\u0314', '\u0315', '\u0316', '\u0317',
			'\u0318', '\u0319', '\u031b', '\u031c', '\u031d', '\u031e', '\u031f', '\u0320',
			'\u0321', '\u0322', '\u0323', '\u0324', '\u0325', '\u0326', '\u0327', '\u0328',
			'\u0329', '\u032a', '\u032b', '\u032c', '\u032d', '\u032e', '\u032f', '\u0330',
			'\u0331', '\u0332', '\u0333', '\u0334', '\u0335', '\u0336', '\u0337', '\u0338',
			'\u0339', '\u033a', '\u033b', '\u033c', '\u033d', '\u033e', '\u033f', '\u0340',
			'\u0341', '\u0342', '\u0343', '\u0344', '\u0345', '\u0347', '\u0348', '\u0349',
			'\u034a', '\u034b', '\u034c', '\u034d', '\u034e', '\u034f', '\u0350', '\u0351',
			'\u0352', '\u0353', '\u0354', '\u0355', '\u0356', '\u0357', '\u0358', '\u0359',
			'\u035a', '\u035b', '\u035c', '\u035d', '\u035e', '\u035f', '\u0360', '\u0361',
			'\u0362', '\u0363', '\u0364', '\u0365', '\u0366', '\u0367', '\u0368', '\u0369',
			'\u036a', '\u036b', '\u036c', '\u036d', '\u036e', '\u036f', '\u0489'
		};

		public RandomService Random { get; set; }

		[SlashCommand("space", "S P A C E  M E M E  D U D E.")]
		public async Task Space(string text)
		{
			await RespondAsync(string.Join(' ', text.LazySubstring(0, 1999).ToCharArray()).ToUpper());
		}

		[SlashCommand("fancy", "Regional indicator emojis.")]
		public async Task Fancy(string text)
		{
			string build = "";

			foreach (char c in text.ToLower())
			{
				if (FancyDic.TryGetValue(c, out string s))
				{
					build += s + '\u200b';
				}
			}

			await RespondAsync(build.LazySubstring(0, 2000));
		}

		[SlashCommand("zalgo", "Creepypasta style text.")]
		public async Task Zalgo(string text)
		{
			string build = "";

			foreach (char c in text)
			{
				build += c;

				for (int i = 0; i < Random.Next(3, 50); i++)
				{
					build += Random.Pick(ZalgoChars);
				}
			}

			await RespondAsync(build);
		}
	}
}