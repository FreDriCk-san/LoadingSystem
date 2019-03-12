using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
	public class ConvertToHTML
	{

		public ConvertToHTML()
		{

		}

		public static async Task ProceedDataTable(DataTable dataTable, int columns, string path)
		{
			var headerBuilder = new StringBuilder();
			headerBuilder.AppendLine("<!DOCTYPE html>");
			headerBuilder.AppendLine("<html>");
			headerBuilder.AppendLine("<head>");
			headerBuilder.AppendLine("<meta charset=\"UTF - 8\">");
			headerBuilder.AppendLine("</head>");
			headerBuilder.AppendLine("<body>");
			headerBuilder.AppendLine("<table border=\"1\">");
			headerBuilder.AppendLine("<tr>");

			for (int i = 1; i <= columns; ++i)
			{
				headerBuilder.AppendLine($"<th>{i}</th>");
			}

			headerBuilder.AppendLine("</tr>");

			var rows = dataTable.Rows.Count;

			using (var writer = new StreamWriter(path, true, Encoding.UTF8))
			{
				await writer.WriteLineAsync(headerBuilder.ToString());

				for (int i = 0; i < rows; ++i)
				{
					await writer.WriteAsync("<tr>");

					for (int j = 0; j < columns; ++j)
					{
						await writer.WriteAsync($"<td>{dataTable.Rows[i][j]}</td>");
					}

					await writer.WriteLineAsync("</tr>");
				}

				await writer.WriteLineAsync("</table>");
				await writer.WriteLineAsync("</body>");
				await writer.WriteLineAsync("</html>");
			}
		}
	}
}
