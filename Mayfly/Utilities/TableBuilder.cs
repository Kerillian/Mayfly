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
		private readonly int[] sizes;
		private readonly List<string[]> tableRows = new List<string[]>();
		private readonly TableDesign design;

		public TableBuilder(TableDesign design, params string[] headers)
		{
			design = design;
			tableHeaders = headers;
			sizes = new int[headers.Length];

			for (int i = 0; i < tableHeaders.Length; i++)
			{
				sizes[i] = tableHeaders[i].Length;
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
					if (row[i].Length > sizes[i])
					{
						sizes[i] = row[i].Length;
					}
				}
				
				tableRows.Add(row);
			}
		}

		private string BuildTemplate()
		{
			string[] parts = new string[sizes.Length];

			for (int i = 0; i < sizes.Length; i++)
			{
				parts[i] = $"{{{i},-{sizes[i]}}}";
			}
			
			return $"{design.Vertical} {string.Join($" {design.Vertical} ", parts)} {design.Vertical}\n";
		}

		private string BuildFiller(char left, char connector, char right)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(left);

			for (int i = 0; i < sizes.Length; i++)
			{
				builder.Append(new string(design.Horizontal, sizes[i] + 2));

				if (i != sizes.Length - 1)
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

			builder.AppendLine(BuildFiller(design.TopLeft, design.TopConnector, design.TopRight));
			builder.AppendFormat(template, tableHeaders);
			builder.AppendLine(BuildFiller(design.HeaderLeft, design.HeaderConnector, design.HeaderRight));

			foreach (string[] row in tableRows)
			{
				builder.AppendFormat(template, row);
			}

			builder.AppendLine(BuildFiller(design.BottomLeft, design.BottomConnector, design.BottomRight));
			return builder.ToString();
		}
	}
}