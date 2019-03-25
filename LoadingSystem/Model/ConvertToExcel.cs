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

		public static Task<bool> ReadData(DataTable dataTable, ExcelPackage excelPackage)
		{
			return Task<bool>.Factory.StartNew(() =>
			{
				var workSheets = excelPackage.Workbook.Worksheets.Add("ImportData");

				// TO DO: Set style or format for output
				workSheets.Cells["A1"].LoadFromDataTable(dataTable, true, OfficeOpenXml.Table.TableStyles.Medium9);

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
