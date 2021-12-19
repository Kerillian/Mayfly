using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Mayfly.Readers
{
	public class ColorReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (input.Contains(','))
			{
				string[] values = input.Split(',');

				if (input.Length > 2)
				{
					if (byte.TryParse(values[0], out byte r) && byte.TryParse(values[1], out byte g) && byte.TryParse(values[2], out byte b))
					{
						return Task.FromResult(TypeReaderResult.FromSuccess(new Color(r, g, b)));
					}
				}
			}
			else
			{
				if (uint.TryParse(input, out uint dec) || uint.TryParse(input.Replace("#", ""), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out dec))
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(new Color(dec)));
				}
			}

			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a Color."));
		}
	}
}