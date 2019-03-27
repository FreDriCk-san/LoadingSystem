using System.ComponentModel;

namespace LoadingSystem.Model
{
	public class DataModel
	{
		// Массив массивов с числами
		private double[][] arrayOfNumbers;
		// Разделитель между значениями
		private char separator;
		// Разделитель десятичного числа
		private char decimalSeparator;
		// Количество столбцов
		private int columnCount;
		// Данные начинаются со строки...
		private int dataStartsFrom;
		// Null значения таблицы
		private double nullValue;
		// Наименование скважины
		private string wellName;
		// Наименование месторождения
		private string fieldName;
		// Наименование дата-сета
		private string dataSetName;
		// Наименование куста
		private string bushName;
		// Кол-во рабочих областей
		private int countOfWorkSpaces;


		#region Data Init
		public double[][] ArrayOfNumbers
		{
			get { return arrayOfNumbers; }

			set
			{
				arrayOfNumbers = value;
				OnPropertyChanged("ArrayOfNumbers");
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

		public double NullValue
		{
			get { return nullValue; }

			set
			{
				nullValue = value;
				OnPropertyChanged("NullValue");
			}
		}

		public string WellName
		{
			get { return wellName; }

			set
			{
				wellName = value;
				OnPropertyChanged("WellName");
			}
		}

		public string FieldName
		{
			get { return fieldName; }

			set
			{
				fieldName = value;
				OnPropertyChanged("FieldName");
			}
		}

		public string DataSetName
		{
			get { return dataSetName; }

			set
			{
				dataSetName = value;
				OnPropertyChanged("DataSetName");
			}
		}

		public string BushName
		{
			get { return bushName; }

			set
			{
				bushName = value;
				OnPropertyChanged("BushName");
			}
		}

		public int CountOfWorkSpaces
		{
			get { return countOfWorkSpaces; }

			set
			{
				countOfWorkSpaces = value;
				OnPropertyChanged("CountOfWorkSpaces");
			}
		}
		#endregion

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
