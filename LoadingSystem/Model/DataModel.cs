using System.Collections.Generic;
using System.ComponentModel;

namespace LoadingSystem.Model
{
	public class DataModel : INotifyPropertyChanged
	{
		private List<double> listOfNumbers;
		private int decimalRound;
		private char separator;
		private char decimalSeparator;
		private int columnCount;
		private int dataStartsFrom;


		public List<double> ListOfNumbers
		{
			get { return listOfNumbers; }

			set
			{
				listOfNumbers = value;
				OnPropertyChanged("ListOfNumbers");
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
            listOfNumbers = new List<double>();
        }


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
