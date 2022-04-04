using System.Drawing;
using DiscordColor = Discord.Color;

namespace Mayfly.Utilities
{
	public static class ColorUtility
	{
		public static int ColorDifference(Color color1, Color color2)
		{
			int r = Math.Abs(color1.R - color2.R);
			int g = Math.Abs(color1.G - color2.G);
			int b = Math.Abs(color1.B - color2.B);
			
			return r + g + b;
		}

		public static IEnumerable<Color> GetKnownColors()
		{
			List<Color> colors = new List<Color>();

			for (KnownColor knownColor = KnownColor.AliceBlue; knownColor <= KnownColor.YellowGreen; knownColor++)
			{
				Color color = Color.FromKnownColor(knownColor);
				colors.Add(color);
			}

			return colors;
		}

		public static string FindClosestKnownColor(DiscordColor color)
		{
			IEnumerable<Color> colors = GetKnownColors();
			Color sysColor = Color.FromArgb(255, color.R, color.G, color.B);

			return colors.Aggregate(Color.Black, (accu, curr) => ColorDifference(sysColor, curr) < ColorDifference(sysColor, accu) ? curr : accu).Name;
		}
	}
}