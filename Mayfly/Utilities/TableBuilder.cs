using System.Text;

namespace Mayfly.Utilities
{
	public class TableBuilder
	{
		private readonly string[] tableHeaders;
		private readonly int[] sizes;
		private readonly List<string[]> tableRows = new List<string[]>();

		public TableBuilder(params string[] headers)
		{
			this.tableHeaders = headers;
			this.sizes = new int[headers.Length];

			for (int i = 0; i < this.tableHeaders.Length; i++)
			{
				this.sizes[i] = this.tableHeaders[i].Length;
			}
		}

		public void AddRow(params string[] row)
		{
			if (row.Length == tableHeaders.Length)
			{
				for (int i = 0; i < row.Length; i++)
				{
					if (row[i].Length > this.sizes[i])
					{
						this.sizes[i] = row[i].Length;
					}
				}
				
				this.tableRows.Add(row);
			}
		}

		private string BuildTemplate()
		{
			string[] parts = new string[this.sizes.Length];

			for (int i = 0; i < this.sizes.Length; i++)
			{
				parts[i] = $"{{{i},-{this.sizes[i]}}}";
			}

			return "| " + string.Join(" | ", parts) + " |\n";
		}

		public string Build()
		{
			StringBuilder builder = new StringBuilder();
			string template = BuildTemplate();
			
			builder.AppendFormat(template, this.tableHeaders);
			builder.AppendFormat(template, this.sizes.Select(x => new string('-', x)).ToArray());

			foreach (string[] row in this.tableRows)
			{
				builder.AppendFormat(template, row);
			}

			return builder.ToString();
		}
	}
}