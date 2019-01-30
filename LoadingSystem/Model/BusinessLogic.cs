using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
    public class BusinessLogic
    {

        public static async Task<string> ReadFromFileAsync(string path)
        {
            using (var reader = File.OpenText(path))
            {
                var text = await reader.ReadToEndAsync();
                return text;
            }
        }



        public static string InfoStartsFrom(string text, int numberOfLine)
        {
            var lines = Regex.Split(text, "\r\n|\r|\n").Skip(numberOfLine - 1);
            var result = string.Join(Environment.NewLine, lines.ToArray());
            return result.Remove(result.LastIndexOf(Environment.NewLine));
        }



        public static int CountOfColumns(string text)
        {
            var result = 0;
            var firstLine = text.Substring(0, text.IndexOf(Environment.NewLine));

            for (int i = 0; i < firstLine.Length - 1; ++i)
            {
                if ((firstLine.ElementAt(i) == ' ') && (char.IsDigit(firstLine.ElementAt(i + 1))))
                {
                    result++;
                }
            }

            return result;
        }



        public static int CountOfRows(string text)
        {
            var result = text.Split('\n').Length;
            return result;
        }



        public static List<double> GetValues(string text, char decimalSeparator, char separator)
        {
            var decimalList = new List<double>();
            var builder = new StringBuilder();

            var numbers = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var number in numbers)
            {
                foreach (var character in number)
                {
                    if (char.IsDigit(character) || character == decimalSeparator)
                    {
                        builder.Append(character);
                    }
                    else if (character == separator)
                    {
                        decimalList.Add(StringToDouble(builder.ToString()));

                        builder.Clear();
                    }
                }

                decimalList.Add(StringToDouble(builder.ToString()));

                builder.Clear();
            }

            return decimalList;
        }


        public static int GetDecimalNumberCount(string text, char decimalSeparator, char separator)
        {
            var counter = 0;
            var firstLine = text.Substring(0, text.IndexOf(Environment.NewLine));
            var flag = false;
            var listOfValues = new List<int>();

            for (int i = 0; i < firstLine.Length; ++i)
            {
                if(firstLine.ElementAt(i) == decimalSeparator)
                {
                    flag = true;
                    counter--;
                }
                else if (firstLine.ElementAt(i) == separator)
                {
                    listOfValues.Add(counter);
                    flag = false;
                    counter = 0;
                }

                if (flag)
                {
                    counter++;

                    if (i == firstLine.Length - 1)
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
                throw new Exception($"Cannot parse to double value {text}");
            }

            return result;
        }
    }
}