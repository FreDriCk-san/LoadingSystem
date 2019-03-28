using OfficeOpenXml;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
	public class ConvertToExcel
	{
		public ConvertToExcel()
		{

		}

		public static Task<bool> ReadData(DataTable dataTable, ExcelPackage excelPackage, string fileName)
		{
			return Task<bool>.Factory.StartNew(() =>
			{
				var workSheets = excelPackage.Workbook.Worksheets.Add(fileName);

				// TO DO: Set style or format for output
				for (int i = 1; i <= dataTable.Columns.Count; ++i)
				{
					workSheets.SetValue(1, i, $"H{i}");
					workSheets.Cells[1, i].Style.Font.Bold = true;
					workSheets.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
					workSheets.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.RoyalBlue);
					workSheets.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);
				}
				workSheets.Cells["A2"].LoadFromDataTable(dataTable, false, OfficeOpenXml.Table.TableStyles.Medium9);

				return true;
			});
		}

		public static Task<bool> SaveAsExcel(string dialogPath, ExcelPackage excelPackage)
		{
			return Task<bool>.Factory.StartNew(() =>
			{
				var path = new FileInfo(dialogPath);

				excelPackage.SaveAs(path);

				return true;
			});
		}
	}
}
