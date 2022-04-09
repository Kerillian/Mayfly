using System.Text;

namespace Mayfly.Utilities
{
	public class TableDesign
	{
		public char Horizontal { get; set; }
		public char Vertical { get; set; }
		public char TopLeft { get; set; }
		public char TopConnector { get; set; }
		public char TopRight { get; set; }
		public char HeaderLeft { get; set; }
		public char HeaderConnector { get; set; }
		public char HeaderRight { get; set; }
		public char BottomLeft { get; set; }
		public char BottomConnector { get; set; }
		public char BottomRight { get; set; }

		public static TableDesign Default { get; } = new TableDesign()
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

		public TableBuilder(params string[] headers) : this(TableDesign.Default, headers)
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

			for (int i = 0; i < tableWidths.Length - 1; i++)
			{
				builder.Append(new string(tableDesign.Horizontal, tableWidths[i] + 2));

				if (i != tableWidths.Length - 1)
				{
					builder.Append(connector);
				}
			}
			
			builder.Append(new string(tableDesign.Horizontal, tableWidths[tableWidths.Length] + 2));
			builder.Append(connector);
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