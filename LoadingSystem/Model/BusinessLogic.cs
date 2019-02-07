using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LoadingSystem.Model
{
	public class BusinessLogic
    {

        public static List<DataModel> ReadData(string[] arrayOfText, int dataIndex, int take)
        {
			var line = string.Empty;
			var listOfData = new List<DataModel>();

            for (int i = dataIndex; i < dataIndex + take; ++i)
            {
                listOfData.Add(GetValues($"{arrayOfText[i]}  "));
            }

			return listOfData;
        }


        public static DataModel GetValues(string text)
        {
            var decimalList = new List<double>();
            var builder = new StringBuilder();
			var dataClass = new DataModel();
			var decimalSeparator = char.MinValue;
			var separator = char.MinValue;
			var decimalRound = 0;

			var lines = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				for (int i = 1; i < line.Length - 2; i++)
				{
					var currentChar = line.ElementAt(i);
					var nextChar = line.ElementAt(i + 1);

					// Check, if it's a header
					if (currentChar == ':' || nextChar == ':')
					{
						builder.Clear();
						continue;
					}

					// Check, if it's a separator
					else if (currentChar == separator && char.IsDigit(line.ElementAt(i - 1)))
					{
						decimalList.Add(StringToDouble(builder.ToString()));
						builder.Clear();
					}

					// Check if it's a decimal separator
					else if (currentChar == '.' || currentChar == ',')
					{
						// Get decimal separator
						if (decimalSeparator == char.MinValue)
						{
							decimalSeparator = currentChar;
						}

						builder.Append(currentChar);
					}

					// Check, if it's a number
					else if (char.IsDigit(currentChar))
					{
						// If it is a last character without next spaces (symbols)
						if (i == line.Length - 3)
						{
							builder.Append(currentChar);
							decimalList.Add(StringToDouble(builder.ToString()));
							decimalRound = GetDecimalNumberCount(builder.ToString(), decimalSeparator, separator);
							builder.Clear();
							continue;
						}

						// Get separator
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

			}

			dataClass.ListOfNumbers = decimalList;
			dataClass.Separator = separator;
			dataClass.DecimalSeparator = decimalSeparator;
			dataClass.DecimalRound = decimalRound;

            return dataClass;
        }


        public static int GetDecimalNumberCount(string text, char decimalSeparator, char separator)
        {
            var counter = 0;
            var flag = false;
            var listOfValues = new List<int>();

            for (int i = 0; i < text.Length; ++i)
            {
                if(text.ElementAt(i) == decimalSeparator)
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



        public static int GetDataIndex(string[] arrayOfText)
        {
            for (int i = 0; i < arrayOfText.Length; ++i)
            {
                if (arrayOfText[i].Contains("Log Data"))
                {
                    return i + 1;
                }
            }

            return 0;
        }


        private static double StringToDouble(string text)
        {
            double result;

            if (!Double.TryParse(text, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
            {
                throw new Exception($"Cannot parse to double value {text}");
            }

            return result;
        }
    }
}