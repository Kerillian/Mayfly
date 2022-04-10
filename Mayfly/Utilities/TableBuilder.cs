using System.Text;

namespace Mayfly.Utilities
{
	public readonly struct TableDesign
	{
		public char Horizontal { get; init; }
		public char Vertical { get; init; }
		public char TopLeft { get; init; }
		public char TopConnector { get; init; }
		public char TopRight { get; init; }
		public char HeaderLeft { get; init; }
		public char HeaderConnector { get; init; }
		public char HeaderRight { get; init; }
		public char BottomLeft { get; init; }
		public char BottomConnector { get; init; }
		public char BottomRight { get; init; }

		public static TableDesign Light { get; } = new TableDesign()
		{
			Horizontal = '─',
			Vertical = '│',
			TopLeft = '╭',
			TopConnector = '┬',
			TopRight = '╮',
			HeaderLeft = '├',
			HeaderConnector = '┼',
			HeaderRight = '┤',
			BottomLeft = '╰',
			BottomConnector = '┴',
			BottomRight = '╯'
		};

		public static TableDesign Heavy { get; } = new TableDesign()
		{
			Horizontal = '━',
			Vertical = '┃',
			TopLeft = '┏',
			TopConnector = '┳',
			TopRight = '┓',
			HeaderLeft = '┣',
			HeaderConnector = '╋',
			HeaderRight = '┫',
			BottomLeft = '┗',
			BottomConnector = '┻',
			BottomRight = '┛'
		};
	}

	public class TableBuilder
	{
		private readonly string[] tableHeaders;
		private readonly int[] tableWidths;
		private readonly List<string[]> tableRows = new List<string[]>();
		private readonly TableDesign tableDesign;

		public TableBuilder(TableDesign design, params string[] headers)
		{
			tableDesign = design;
			tableHeaders = headers;
			tableWidths = new int[headers.Length];

			for (int i = 0; i < tableHeaders.Length; i++)
			{
				tableWidths[i] = tableHeaders[i].Length;
			}
		}

		public TableBuilder(params string[] headers) : this(TableDesign.Light, headers)
		{
			
		}

		public void AddRow(params string[] row)
		{
			if (row.Length == tableHeaders.Length)
			{
				for (int i = 0; i < row.Length; i++)
				{
					if (row[i].Length > tableWidths[i])
					{
						tableWidths[i] = row[i].Length;
					}
				}
				
				tableRows.Add(row);
			}
		}

		private string BuildTemplate()
		{
			string[] parts = new string[tableWidths.Length];

			for (int i = 0; i < tableWidths.Length; i++)
			{
				parts[i] = $"{{{i},-{tableWidths[i]}}}";
			}
			
			return $"{tableDesign.Vertical} {string.Join($" {tableDesign.Vertical} ", parts)} {tableDesign.Vertical}\n";
		}

		private string BuildFiller(char left, char connector, char right)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(left);

			for (int i = 0; i < tableWidths.Length; i++)
			{
				builder.Append(new string(tableDesign.Horizontal, tableWidths[i] + 2));

				if (i != tableWidths.Length - 1)
				{
					builder.Append(connector);
				}
			}
			
			return builder.Append(right).ToString();
		}

		public string Build()
		{
			StringBuilder builder = new StringBuilder();
			string template = BuildTemplate();

			builder.AppendLine(BuildFiller(tableDesign.TopLeft, tableDesign.TopConnector, tableDesign.TopRight));
			builder.AppendFormat(template, tableHeaders);
			builder.AppendLine(BuildFiller(tableDesign.HeaderLeft, tableDesign.HeaderConnector, tableDesign.HeaderRight));

			foreach (string[] row in tableRows)
			{
				builder.AppendFormat(template, row);
			}

			builder.AppendLine(BuildFiller(tableDesign.BottomLeft, tableDesign.BottomConnector, tableDesign.BottomRight));
			return builder.ToString();
		}
	}
}