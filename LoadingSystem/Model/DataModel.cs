using System.Collections.Generic;
using System.ComponentModel;

namespace LoadingSystem.Model
{
	public class DataModel
	{
		// Массив массивов с числами
		private double[][] arrayOfNumbers;
		// Округление десятичного числа
		private int decimalRound;
		// Разделитель между значениями
		private char separator;
		// Разделитель десятичного числа
		private char decimalSeparator;
		// Количество столбцов
		private int columnCount;
		// Данные начинаются со строки...
		private int dataStartsFrom;


		public double[][] ArrayOfNumbers
		{
			get { return arrayOfNumbers; }

			set
			{
				arrayOfNumbers = value;
				OnPropertyChanged("ArrayOfNumbers");
			}
		}

		public int DecimalRound
		{
			get { return decimalRound; }

			set
			{
				decimalRound = value;
				OnPropertyChanged("DecimalRound");
			}
		}

		public char Separator
		{
			get { return separator; }

			set
			{
				separator = value;
				OnPropertyChanged("Separator");
			}
		}

		public char DecimalSeparator
		{
			get { return decimalSeparator; }

			set
			{
				decimalSeparator = value;
				OnPropertyChanged("DecimalSeparator");
			}
		}

		public int ColumnCount
		{
			get { return columnCount; }

			set
			{
				columnCount = value;
				OnPropertyChanged("ColumnCount");
			}
		}

		public int DataStartsFrom
		{
			get { return dataStartsFrom; }

			set
			{
				dataStartsFrom = value;
				OnPropertyChanged("DataStartsFrom");
			}
		}

        public DataModel()
        {

        }


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
