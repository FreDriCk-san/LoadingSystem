using OfficeOpenXml;
using System.Collections.Generic;
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



		public static Task<bool> ReadData(DataTable dataTable, ExcelPackage excelPackage, string fileName, List<string> listOfCurveNames)
		{
			return Task<bool>.Factory.StartNew(() =>
			{
				var workSheets = excelPackage.Workbook.Worksheets.Add(fileName);

                // Set headers
                for (int i = 1; i <= dataTable.Columns.Count; ++i)
                {
                    var index = int.Parse(dataTable.Columns[i - 1].Caption);

                    workSheets.SetValue(1, i, $"H{dataTable.Columns[i - 1].Caption}");
                    workSheets.Cells[1, i].Style.Font.Bold = true;
                    workSheets.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    workSheets.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.RoyalBlue);
                    workSheets.Cells[1, i].Style.Font.Color.SetColor(System.Drawing.Color.White);

                    // Set properties
                    if (null == listOfCurveNames)
                    {
                        if (index == 0)
                        {
                            workSheets.SetValue(2, 1, "Depth");
                        }
                    }
                    else
                    {
                        workSheets.SetValue(2, i, listOfCurveNames[index + 1]);
                        workSheets.Cells[2, i].Style.Font.Italic = true;
                        workSheets.Cells[2, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        workSheets.Cells[2, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SaddleBrown);
                        workSheets.Cells[2, i].Style.Font.Color.SetColor(System.Drawing.Color.WhiteSmoke);
                    }
                }

                // Set data
                workSheets.Cells["A3"].LoadFromDataTable(dataTable, false, OfficeOpenXml.Table.TableStyles.Medium9);

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
