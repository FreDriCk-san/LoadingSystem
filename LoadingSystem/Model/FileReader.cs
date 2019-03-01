using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
	public static class FileReader
    {
        private const int DefaultBufferSize = 4096;

        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

		// Считывание и обработка входных данных
        public static async Task<DataModel> ReadAllLinesAsync(string path)
        {
			var data = new DataModel();
			var builder = new StringBuilder();
			var decimalSeparator = char.MinValue;
			var separator = char.MinValue;
			var decimalRound = 0;

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				using (var reader = new StreamReader(stream, CodepageDetector.getCyrillic(path)))
				{
                    var dataLineFound = false;
					var readColumnOnce = false;
                    var index = 1;
					var arrayOfNumbers = new double[4096][];
					var arrayNumStep = 0;
					var firstLineProcessed = false;
					var arrayOfPositions = new int[0];
					var prevLine = string.Empty;
					var prevPositions = new int[0];

					// Считывать, пока не конец потока
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();

						// Если не найдена строка, с которой начинаются данные для обработки
						if (!dataLineFound)
						{
							// Если текущая строка содержит слово ASCII
							if (StringIsASCII(line))
							{
								dataLineFound = true;
								readColumnOnce = true;
								data.DataStartsFrom = index;
								continue;
							}

							// Если текущая строка состоит только из чисел и специяльных знаков (пробел, запятая, точка...)
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
							var lineArray = new double[data.ColumnCount];
							var arrayLineStep = 0;

							if (prevLine != string.Empty)
							{
								if (prevLine.Length != line.Length)
								{
									arrayOfPositions = GetNumericPositions(line, data.ColumnCount, separator);
								}
							}

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
								else if ((currentChar == separator && i != 0 && line[i - 1] != ' ')
									|| (i == line.Length - 2 && !char.IsDigit(line[i - 1])))
								{
									lineArray[arrayLineStep] = double.NaN;
									builder.Clear();
									arrayLineStep++;
								}

								// Проверить позицию, после прохода (данные о позициях разделителей хранятся в массиве)
								else if (firstLineProcessed && currentChar == separator)
								{
									// TO DO: Исправить, нужна причина для изменений массива
									if (prevLine.Length == line.Length && (prevLine[i] == ' '))
									{
										arrayOfPositions = GetNumericPositions(line, data.ColumnCount, separator);
									}

									if (arrayOfPositions[arrayLineStep] == i && !char.IsDigit(line[i - 1]))
									{
										lineArray[arrayLineStep] = double.NaN;
										arrayLineStep++;
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
										decimalRound = GetDecimalNumberCount(builder.ToString(), decimalSeparator, separator);
										builder.Clear();
										arrayLineStep++;
										prevLine = line;
										prevPositions = arrayOfPositions;

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
								arrayOfPositions = GetNumericPositions(line, data.ColumnCount, separator);
								prevPositions = arrayOfPositions;
								firstLineProcessed = true;
							}

							arrayOfNumbers[arrayNumStep] = lineArray;
							arrayNumStep++;

							if (arrayNumStep % 4096 == 0 && arrayNumStep > 0)
							{
								Array.Resize(ref arrayOfNumbers, arrayOfNumbers.Length + 4096);
							}
						}

						index++;
					}


					data.Separator = separator;
					data.DecimalRound = decimalRound;
					data.DecimalSeparator = decimalSeparator;
					data.ArrayOfNumbers = arrayOfNumbers;
				}
			}

			return data;
        }


		// Считать определённое количество строк
		public static async Task<string[]> ReadLinesAsync(string path, int toString)
		{
			var result = new string[1024];
			var index = 0;

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			{
				using (var reader = new StreamReader(stream, CodepageDetector.getCyrillic(path)))
				{

					while (index <= toString)
					{
						var line = await reader.ReadLineAsync();	

						result[index] = $"{index}:\t\t{line}";

						index++;
					}
				}
			}

			return result;
		}


		// Определить дробную часть числа
		private static int GetDecimalNumberCount(string text, char decimalSeparator, char separator)
		{
			var counter = 0;
			var flag = false;
			var listOfValues = new List<int>();

			for (int i = 0; i < text.Length; ++i)
			{
				if (text.ElementAt(i) == decimalSeparator)
				{
					flag = true;
					counter--;
				}
				else if (text.ElementAt(i) == separator)
				{
					listOfValues.Add(counter);
					flag = false;
					counter = 0;
				}

				if (flag)
				{
					counter++;

					if (i == text.Length - 1)
					{
						listOfValues.Add(counter);
					}
				}
			}

			var maxCount = 0;

			for (int i = 0; i < listOfValues.Count; ++i)
			{
				if (listOfValues.ElementAt(i) > maxCount)
				{
					maxCount = listOfValues.ElementAt(i);
				}
			}

			return maxCount;
		}



		private static double StringToDouble(string text)
		{
			double result;

			if (!Double.TryParse(text, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
			{
				//throw new Exception($"Cannot parse to double value {text}");
				 return double.NaN;
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
			for (int i = 0; i < text.Length; ++i)
			{
				var currentChar = text[i];
				
                if (!char.IsDigit(currentChar) && currentChar != ' ' 
                    && currentChar != '.' && currentChar != ',' 
                    && currentChar != ';' && currentChar != ':')
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

			for (int i = 0; i < text.Length - 1; ++i)
			{
				if (i == 0 && char.IsDigit(text[0]))
				{
					result++;
				}

				if ((text[i] == ' ') && (char.IsDigit(text[i + 1]) || text[i + 1] != ' '))
				{
					result++;
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



		private static int CountElementsOfArray(int[] array, int element)
		{
			var result = 0;

			foreach (var item in array)
			{
				if (item == element)
				{
					result++;
				}
			}

			return result;
		}



		private static bool CountOfSpacesAreSame(char separator, string line, string previousLine)
		{
			var firstCounter = 0;
			var secondCounter = 0;

			for (int i = 0; i < line.Length - 1; ++i)
			{
				if (line[i] == separator || line[i] == ' ')
				{
					firstCounter++;
				}

				if (previousLine[i] == separator || previousLine[i] == ' ')
				{
					secondCounter++;
				}
			}

			if (firstCounter == secondCounter)
			{
				return true;
			}

			return false;
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
