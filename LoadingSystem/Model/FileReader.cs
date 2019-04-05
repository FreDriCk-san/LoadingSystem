using LoadingSystem.Util;
using NPOI.HSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
	public static class FileReader
    {
        private const int DefaultBufferSize = 4096;

        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;


		// Считывание и обработка входных данных
        public static async Task<DataModel> ReadAllLinesAsync(string path, CancellationToken cancellationToken)
        {
			var data = new DataModel();
			var decimalSeparator = char.MinValue;
			var separator = char.MinValue;

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				using (var reader = new StreamReader(stream, CodepageDetector.getCyrillic(path)))
				{
					var propertyLineFound = false;
                    var readProperty = true;

                    var dataLineFound = false;
					var readColumnOnce = false;
                    var index = 1;

					var arrayOfNumbers = new double[4096][];
					var arrayOfPositions = new int[0];
					var arrayNumStep = 0;
					var firstLineProcessed = false;

					// Считывать, пока не конец потока
					while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
					{
						var line = $"{await reader.ReadLineAsync()} ";


                        // Если не найдена строка, с которой начинаются данные для обработки
                        if (!dataLineFound)
						{
                            // Если не найдена строка, с которой начинаются свойства
                            if (!propertyLineFound)
                            {
                                if (StringIsProperty(line, "~WELL", "~well", "~Well"))
                                {
                                    propertyLineFound = true;
                                }
                            }
                            else if (propertyLineFound && readProperty)
                            {
                                if (line.Length == 0)
                                {
                                    readProperty = false;
                                }
                                else if (StringIsProperty(line, "NULL", "null", "Null"))
                                {
                                    data.NullValue = StringToDouble(InspectPropertyLine(PropertyEnum.Null, line));
                                }
                                else if (StringIsProperty(line, "WELL", "well", "Well"))
                                {
                                    data.WellName = InspectPropertyLine(PropertyEnum.WellName, line);
                                }
                                else if (StringIsProperty(line, "FIELD", "field", "Field"))
                                {
                                    data.FieldName = InspectPropertyLine(PropertyEnum.FieldName, line);
                                }
                            }

                            // Если текущая строка содержит слово ASCII
                            if (StringIsASCII(line))
							{
								dataLineFound = true;
								readColumnOnce = true;
								data.DataStartsFrom = index;
								continue;
							}

							// Если текущая строка состоит только из чисел и специальных знаков (пробел, запятая, точка...)
							else if (StringIsDigitOnly(line))
							{
								dataLineFound = true;
								readColumnOnce = true;
								data.DataStartsFrom = index;
							}
						}

						// Если нашлась строка с данными, считать один раз и определить количество столбцов
						if (readColumnOnce)
						{
							data.ColumnCount = CountOfColumns(line);
							readColumnOnce = false;
						}


						// Если найдена строка, с которой начинаются данные для обработки
						if (dataLineFound)
						{
							var firstHeuristic = FirstHeuristic(line, separator, decimalSeparator, data.ColumnCount, firstLineProcessed, arrayOfPositions);
							firstLineProcessed = firstHeuristic.Item2;
							arrayOfPositions = firstHeuristic.Item3;
							separator = firstHeuristic.Item4;
							decimalSeparator = firstHeuristic.Item5;

							arrayOfNumbers[arrayNumStep] = firstHeuristic.Item1;
							arrayNumStep++;

							if (arrayNumStep % 4096 == 0 && arrayNumStep > 0)
							{
								Array.Resize(ref arrayOfNumbers, arrayOfNumbers.Length + 4096);
							}
						}

						index++;
					}


					data.Separator = separator;
					data.DecimalSeparator = decimalSeparator;
					data.ArrayOfNumbers = arrayOfNumbers;
				}
			}

			return data;
        }


		// Считать определённое количество строк
		public static async Task<string[]> ReadLinesAsync(string path, int toString, CancellationToken cancellationToken)
		{
			var result = new string[1024];
			var index = 0;

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				using (var reader = new StreamReader(stream, CodepageDetector.getCyrillic(path)))
				{

					while (index <= toString)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}

						var line = await reader.ReadLineAsync();	

						result[index] = $"{index}:\t\t{line}";

						index++;
					}
				}
			}

			return result;
		}



		public static DataModel ReadAsXLSX(string path, int numOfWorkSheet, CancellationToken cancellationToken)
		{
			// Max row count:	 ~1048576
			// Max column count: ~16384

			var arrayOfNumbers = new double[4096][];
			var data = new DataModel();
			var maxColumnCount = 0;
			var rowIndex = 0;
			var countOfWorkSheets = 0;
			var arrayOfNames = new string[4096];
            var builder = new StringBuilder();

            if (!CanReadFromFile(path))
			{
				return null;
			}

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				using (var package = new ExcelPackage(stream))
				{
					foreach (var item in package.Workbook.Worksheets)
					{
						if (countOfWorkSheets % 4096 == 0 && countOfWorkSheets > 0)
						{
							Array.Resize(ref arrayOfNames, arrayOfNames.Length + 4096);
						}

						arrayOfNames[countOfWorkSheets] = item.Name;
						countOfWorkSheets++;
					}

					var workSheet = package.Workbook.Worksheets[numOfWorkSheet];
					var lineOfNumbersFound = false;
                    var lineOfPropertiesFound = false;

					if (null != workSheet.Dimension)
					{
						// loop all rows
						var startRow = workSheet.Dimension.Start.Row;
						var endRow = workSheet.Dimension.End.Row;
						for (int i = startRow; i <= endRow; ++i)
						{
							var lineArray = new double[4096];
							var lineStep = 0;

							// loop all columns in a row
							var startColumn = workSheet.Dimension.Start.Column;
							var endColumn = workSheet.Dimension.End.Column;
							for (int j = startColumn; j <= endColumn; ++j)
							{
								var cellValue = workSheet.Cells[i, j].Value;

								if (null != cellValue)
								{
                                    var cellContent = cellValue.ToString();

                                    if (cellContent != string.Empty)
                                    {
                                        builder.Append($"{cellContent},");
                                    }


                                    if (cellContent.Equals("depth", StringComparison.InvariantCultureIgnoreCase) 
                                        || cellContent.Equals("dp", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        lineOfPropertiesFound = true;
                                    }


									if (lineOfNumbersFound)
									{
										lineArray[lineStep] = StringToDouble(cellContent);
										lineStep++;
									}
									else if (double.MinValue != StringToDouble(cellContent))
									{
										lineArray[lineStep] = StringToDouble(cellContent);
										lineStep++;
										lineOfNumbersFound = true;
									}
								}


                                if (lineStep % 4096 == 0 && lineStep > 0)
								{
									Array.Resize(ref lineArray, lineArray.Length + 4096);
								}
							}


                            if (lineOfPropertiesFound)
                            {
                                data.ListOfCurveNames = GetPropertiesFromRowLine(builder);
                                lineOfPropertiesFound = false;
                            }


							if (lineOfNumbersFound)
							{
								if (lineStep > maxColumnCount)
								{
									maxColumnCount = lineStep;
								}

								if (rowIndex % 4096 == 0 && rowIndex > 0)
								{
									Array.Resize(ref arrayOfNumbers, arrayOfNumbers.Length + 4096);
								}

								arrayOfNumbers[rowIndex] = lineArray;
								rowIndex++;
							}

                            builder.Clear();
						}
					}
				}

			}

			data.ArrayOfNumbers = arrayOfNumbers;
			data.ColumnCount = maxColumnCount;
			data.CountOfWorkSpaces = countOfWorkSheets;
			data.ArrayOfWorkSheetsName = arrayOfNames;

			return data;
		}


		// TO DO: Get position of row with information
		public static DataModel ReadAsXLS(string path, int numOfWorkSheet, CancellationToken cancellationToken)
		{
			// Max row count:	 ~65536
			// Max column count: ~256

			var arrayOfNumbers = new double[4096][];
			var data = new DataModel();
			var maxColumnCount = 0;
			var rowIndex = 0;
			var arrayOfNames = new string[4096];
            var builder = new StringBuilder();
            HSSFWorkbook workBook;

			if (!CanReadFromFile(path))
			{
				return null;
			}

			using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				workBook = new HSSFWorkbook(file);
			}

			var workSheet = workBook.GetSheetAt(numOfWorkSheet);
			var rows = workSheet.GetEnumerator();
			var countOfSheets = workBook.NumberOfSheets;

			for (int i = 0; i < countOfSheets; ++i)
			{
				if (i % 4096 == 0 && i > 0)
				{
					Array.Resize(ref arrayOfNames, arrayOfNames.Length + 4096);
				}

				var workSheetName = workBook.GetSheetName(i);
				arrayOfNames[i] = workSheetName;
			}



			while (rows.MoveNext())
			{
				var row = (HSSFRow)rows.Current;
				var lineOfNumbersFound = false;
                var lineOfPropertiesFound = false;
                var lineArray = new double[256];
				var lineStep = 0;

				for (int j = 0; j < row.LastCellNum; ++j)
				{
					var cellValue = row.GetCell(j);

					if (null != cellValue)
					{
                        var cellContent = cellValue.ToString();

                        if (StringIsProperty(cellContent, "WELL", "well", "Well"))
						{
							data.WellName = cellContent;
						}

                        if (cellContent != string.Empty)
                        {
                            builder.Append($"{cellContent},");
                        }

                        
                        if (cellContent.Equals("depth", StringComparison.InvariantCultureIgnoreCase)
                            || cellContent.Equals("dp", StringComparison.InvariantCultureIgnoreCase))
                        {
                            lineOfPropertiesFound = true;
                        }



						if (StringToDouble(cellContent) != double.MinValue)
						{
							lineArray[lineStep] = StringToDouble(cellContent);
							lineStep++;
							lineOfNumbersFound = true;
						}
						else if (cellValue.CellType == NPOI.SS.UserModel.CellType.Formula)
						{
							if (cellValue.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
							{
								lineArray[lineStep] = cellValue.NumericCellValue;
								lineStep++;
								lineOfNumbersFound = true;
							}
						}
					}


                }


                if (lineOfPropertiesFound)
                {
                    data.ListOfCurveNames = GetPropertiesFromRowLine(builder);
                    lineOfPropertiesFound = false;
                }


				if (lineOfNumbersFound)
				{
					if (lineStep > maxColumnCount)
					{
						maxColumnCount = lineStep;
					}

					if (rowIndex % 4096 == 0 && rowIndex > 0)
					{
						Array.Resize(ref arrayOfNumbers, arrayOfNumbers.Length + 4096);
					}

					arrayOfNumbers[rowIndex] = lineArray;
					rowIndex++;
				}

                builder.Clear();
			}



			data.ArrayOfNumbers = arrayOfNumbers;
			data.ColumnCount = maxColumnCount;
			data.CountOfWorkSpaces = countOfSheets;
			data.ArrayOfWorkSheetsName = arrayOfNames;

			return data;
		}



		public static Task<string[]> ReadLinesFromXLSX(string path, int toRow, int numOfWorkSheet, CancellationToken cancellationToken)
		{
			return Task<string[]>.Factory.StartNew(() =>
			{
				if (!CanReadFromFile(path))
				{
					return null;
				}

				var result = new string[1024];
				var index = 0;

				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
				{
					using (var package = new ExcelPackage(stream))
					{
						var workSheet = package.Workbook.Worksheets[numOfWorkSheet];

						if (null != workSheet.Dimension)
						{

							// loop all rows
							var startRow = workSheet.Dimension.Start.Row;
							var endRow = workSheet.Dimension.End.Row;
							for (int i = startRow; i <= endRow; ++i)
							{
								if (index > toRow)
								{
									return result;
								}

								var builder = new StringBuilder();
								// loop all columns in a row
								var startColumn = workSheet.Dimension.Start.Column;
								var endColumn = workSheet.Dimension.End.Column;
								for (int j = startColumn; j <= endColumn; ++j)
								{
									var cellValue = workSheet.Cells[i, j].Value;

									if (null != cellValue)
									{
										builder.Append($"|{cellValue.ToString()}|\t");
									}
								}

								result[index] = $"{index}:\t\t{builder.ToString()}";
								index++;
							}
						}
					}
				}

				return result;
			});
			
		}



		public static Task<string[]> ReadLinesFromXLS(string path, int toRow, int numOfWorkSheet, CancellationToken cancellationToken)
		{
			return Task<string[]>.Factory.StartNew(() =>
			{
				if (!CanReadFromFile(path))
				{
					return null;
				}

				var result = new string[1024];
				var index = 0;
				HSSFWorkbook workBook;

				using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
				{
					workBook = new HSSFWorkbook(file);
				}

				var workSheet = workBook.GetSheetAt(numOfWorkSheet);
				var rows = workSheet.GetEnumerator();

				while (rows.MoveNext() && index < toRow)
				{
					var builder = new StringBuilder();
					var row = (HSSFRow)rows.Current;

					for (int j = 0; j < row.LastCellNum; ++j)
					{
						var cellValue = row.GetCell(j);

						if (null != cellValue)
						{
							if (cellValue.CellType == NPOI.SS.UserModel.CellType.Formula)
							{
								if (cellValue.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
								{
									builder.Append($"|{cellValue.NumericCellValue}|\t");
								}
								else if (cellValue.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String)
								{
									builder.Append($"|{cellValue.StringCellValue}|\t");
								}
							}
							else
							{
								builder.Append($"|{cellValue.ToString()}|\t");
							}
						}
					}

					result[index] = $"{index}:\t\t{builder.ToString()}";
					index++;
				}

				return result;
			});
		}



		private static double StringToDouble(string text)
		{
			double result;

			if (!Double.TryParse(text.Replace(',','.'), NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result))
			{
				 return double.MinValue;
			}

			return result;
		}



		private static bool StringIsDigitOnly(string text)
		{
            // If current string is null or contains only spaces
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            // If current string contains typical table symbols or numbers
			for (int i = 0; i < text.Length - 1; ++i)
			{
				var currentChar = text[i];
				
				if (currentChar == '-' && text[i+1] == '-')
				{
					return false;
				}

                if (!char.IsDigit(currentChar) && currentChar != ' ' 
                    && currentChar != '.' && currentChar != ',' 
                    && currentChar != ';' && currentChar != ':'
					&& currentChar != '-' && currentChar != '\t'
					&& currentChar != '|')
                {
                    return false;
                }

			}

			return true;
		}



		private static bool StringIsASCII(string text)
		{
			// If current string is null or contains only spaces
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			if (!text.Contains("~ASCII") && !text.Contains("~ascii") && !text.Contains("~Ascii"))
			{
				return false;
			}

			return true;
		}



		private static int CountOfColumns(string text)
		{
			var result = 0;

			if (text.Contains('|'))
			{
				for (int i = 1; i < text.Length; ++i)
				{
					if ((text[i]) == '|' && text[i - 1] == ' ')
					{
						result++;
					}
				}
			}
			else
			{
				for (int i = 0; i < text.Length - 1; ++i)
				{
					if (i == 0 && char.IsDigit(text[0]))
					{
						result++;
					}

					if (((text[i] == ' ') && (char.IsDigit(text[i + 1]) || text[i + 1] != ' '))
						|| (text[i] == '\t' && char.IsDigit(text[i + 1])))
					{
						result++;
					}
				}
			}


			return result;
		}



		private static int[] GetNumericPositions(string line, int columnCount, char separator)
		{
			var result = new int[columnCount];
			var arrayStep = 0;

			for (int i = 1; i < line.Length - 1; ++i)
			{
				if (line[i] == separator && char.IsDigit(line[i - 1]))
				{
					result[arrayStep] = i;
					arrayStep++;
				}
			}

			return result;
		}



		private static bool StringIsProperty(string text, string firstType, string secondType, string thirdType)
		{
			// If current string is null or contains only spaces
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			if (!text.Contains(firstType) && !text.Contains(secondType) && !text.Contains(thirdType))
			{
				return false;
			}

			return true;
		}



		private static string InspectPropertyLine(PropertyEnum property, string line)
		{
			var builder = new StringBuilder();

			if (property == PropertyEnum.Null)
			{
                for (int i = 1; i < line.Length; ++i)
                {
                    if (char.IsDigit(line[i]) || line[i] == '-' || line[i] == ',' || (line[i] == '.' && char.IsDigit(line[i - 1])))
                    {
                        builder.Append(line[i]);
                    }
                }

				return builder.ToString();
			}
			else
			{
				var startRead = false;

				foreach (var character in line)
				{
					if (startRead)
					{
						builder.Append(character);
					}
					else if (character == ':')
					{
						startRead = true;
					}
				}

				return builder.ToString();
			}
		}



		private static bool CanReadFromFile(string path)
		{
			try
			{
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
				{

				}

				return true;
			}
			catch
			{
				return false;
			}
		}



        private static List<string> GetPropertiesFromRowLine(StringBuilder stringBuilder)
        {
            var result = new List<string> { "" };
            var rowContent = stringBuilder.ToString().Split(',');

            foreach (var item in rowContent)
            {
                result.Add(item);
            }

            return result;
        }



		private static Tuple<double[], bool, int[], char, char> FirstHeuristic(string line, char separator, char decimalSeparator, int columnCount, bool firstLineProcessed, int[] positions)
		{
			try
			{
				var builder = new StringBuilder();
				var lineArray = new double[columnCount];
				var arrayLineStep = 0;
				var arrayOfPositions = positions;


				// Обработка текущей строки по символам
				for (int i = 0; i < line.Length - 1; i++)
				{
					var currentChar = line[i];
					var nextChar = line[i + 1];

					// Проверить, если встретился заголовок (Например 1:)
					if (currentChar == ':' || nextChar == ':')
					{
						builder.Clear();
					}

					// Если встретилось отрицательное число
					else if (currentChar == '-' && char.IsDigit(nextChar))
					{
						builder.Append(currentChar);
					}

					// Проверить, если встретился разделитель, перед которым стоит число (если индекс не 0!!)
					else if (currentChar == separator && i != 0 && char.IsDigit(line[i - 1]))
					{
						lineArray[arrayLineStep] = StringToDouble(builder.ToString());
						builder.Clear();
						arrayLineStep++;

						if (arrayLineStep % 4096 == 0 && arrayLineStep > 0)
						{
							Array.Resize(ref lineArray, lineArray.Length + 4096);
						}
					}

					// Если встретился посторонний символ или последний элемент не валиден
					else if ((currentChar == separator && i != 0 && (line[i - 1] != ' ' && line[i - 1] != '|'))
						|| (i == line.Length - 2 && (!char.IsDigit(line[i - 1]) && line[i - 1] != decimalSeparator) && arrayLineStep != lineArray.Length))
					{
						lineArray[arrayLineStep] = double.MinValue;
						builder.Clear();
						arrayLineStep++;
					}

					// Проверить позицию, после прохода (данные о позициях разделителей хранятся в массиве)
					else if (firstLineProcessed && currentChar == separator && arrayLineStep != lineArray.Length)
					{
						if (arrayOfPositions[arrayLineStep] == i)
						{
							if (nextChar != separator && separator == ' ')
							{
								arrayOfPositions = GetNumericPositions(line, columnCount, separator);
							}
							else if (!char.IsDigit(line[i - 1]))
							{
								lineArray[arrayLineStep] = double.MinValue;
								arrayLineStep++;
							}
						}
					}

					// Проверить, если встретился разделитель десятичного числа
					else if (currentChar == '.' || currentChar == ',')
					{
						// Определить разделитель десятичного числа
						if (decimalSeparator == char.MinValue)
						{
							decimalSeparator = currentChar;
						}

						builder.Append(currentChar);
					}

					// Проверить, если встретилось число
					else if (char.IsDigit(currentChar))
					{
						// Если текущий символ является последним и не имеет пробелов
						if (i == line.Length - 2)
						{
							builder.Append(currentChar);
							lineArray[arrayLineStep] = StringToDouble(builder.ToString());
							builder.Clear();
							arrayLineStep++;

							if (arrayLineStep % 4096 == 0 && arrayLineStep > 0)
							{
								Array.Resize(ref lineArray, lineArray.Length + 4096);
							}

							continue;
						}

						// Определить разделитель
						if (!char.IsDigit(nextChar))
						{
							if (separator == char.MinValue && decimalSeparator != char.MinValue)
							{
								separator = nextChar;
							}
						}

						builder.Append(currentChar);
					}

				}

				// Получить шаблон позиций разделителей значений
				if (!firstLineProcessed)
				{
					arrayOfPositions = GetNumericPositions(line, columnCount, separator);
					firstLineProcessed = true;
				}

				return new Tuple<double[], bool, int[], char, char>(lineArray, firstLineProcessed, arrayOfPositions, separator, decimalSeparator);
			}
			catch (Exception exception)
			{
				throw new Exception($"First Heuristic exception:{exception.Message}");
			}
		}
	}




    class CodepageDetector
    {
        public CodepageDetector() { }

        static string[] mCodePages = { "windows-1251", "utf-8", "cp866", "koi8-r" };
        static string mDefaultPage = "us-ascii";
        static Decoder[] mDec = null;
        static char[] mAlphabet = {
            'А','Б','В','Г','Д','Е','Ё','Ж','З','И','Й','К','Л','М','Н','О','П','Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я',
            'а','б','в','г','д','е','ё','ж','з','и','й','к','л','м','н','о','п','р','с','т','у','ф','х','ц','ч','ш','щ','ъ','ы','ь','э','ю','я' };



        static public int getCyrillicQuality(string aStr)
        {
            int result = 0;
            int id = 0;
            while (id < aStr.Length)
            {
                if (aStr.IndexOfAny(mAlphabet, id, 1) >= 0)
                    result++;
                else if (aStr[id] > 127)
                    result--;
                id++;
            }
            return result;
        }



        public static Encoding getCyrillicFromBuffer(byte[] aBuff, int aIndex, int aSize)
        {
            if (mDec == null)
            {
                mDec = new Decoder[mCodePages.Length];
                int id = 0;
                foreach (string i in mCodePages)
                    mDec[id++] = Encoding.GetEncoding(i).GetDecoder();
            }

            int[] counts = new int[mCodePages.Length];
            for (int i = 0; i < mCodePages.Length; i++)
                counts[i] = 0;

            char[] chars = new char[aSize * 2];

            for (int i = 0; i < mCodePages.Length; i++)
            {
                mDec[i].GetChars(aBuff, aIndex, aSize, chars, 0);
                string s = new string(chars);
                counts[i] += getCyrillicQuality(s);
            }

            int idMax = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > counts[idMax])
                    idMax = i;
            }

            return Encoding.GetEncoding(mCodePages[idMax]);
        }



        public static Encoding getCyrillic(string aFilename)
        {
            byte[] buff = new byte[1024 * 8];
            int size = 0;
            try
            {
                FileStream reader = new FileStream(aFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                size = reader.Read(buff, 0, buff.Length);
                reader.Close();
            }
            catch
            {
                return Encoding.GetEncoding(mDefaultPage);
            }
            if (size <= 0)
                return Encoding.GetEncoding(mDefaultPage);
            // detect BOM
            if (size >= 2)
            {
                if ((buff[0] == 0xff) && (buff[1] == 0xfe))
                    return Encoding.GetEncoding("utf-16");
                if ((buff[0] == 0xfe) && (buff[1] == 0xff))
                    return Encoding.GetEncoding("utf-16BE");

            }
            if ((size >= 3) && (buff[0] == 0xef) && (buff[1] == 0xbb) && (buff[2] == 0xbf))
                return Encoding.GetEncoding("utf-8");

            return getCyrillicFromBuffer(buff, 0, size);
        }
    }

    public class DynamicEncoder
    {
        Encoding mEnc = null;
        Encoding mCandidate = null;
        int mCandidateSuccess = 0;
        public string GetString(byte[] aBuff, int aIndex, int aSize)
        {
            if (mEnc != null)
                return mEnc.GetString(aBuff, aIndex, aSize);
            bool isAscii = true;
            for (int i = 0; (i < aSize) && isAscii; i++)
                isAscii = aBuff[aIndex + i] < 128;
            if (isAscii)
                return Encoding.ASCII.GetString(aBuff, aIndex, aSize);
            Encoding newCandidate = CodepageDetector.getCyrillicFromBuffer(aBuff, aIndex, aSize);
            string result = newCandidate.GetString(aBuff, aIndex, aSize);
            bool setCandidate = true;
            if (mCandidate != null)
            {
                string oldResult = mCandidate.GetString(aBuff, aIndex, aSize);
                int newQuality = CodepageDetector.getCyrillicQuality(result);
                int oldQuality = CodepageDetector.getCyrillicQuality(oldResult);
                if (newQuality <= oldQuality)
                {
                    mCandidateSuccess++;
                    setCandidate = false;
                    result = oldResult;
                }
            }
            if (setCandidate)
            {
                mCandidate = newCandidate;
                mCandidateSuccess = 0;
            }
            else if (mCandidateSuccess >= 3)
                mEnc = mCandidate;
            return result;
        }
    }
}
