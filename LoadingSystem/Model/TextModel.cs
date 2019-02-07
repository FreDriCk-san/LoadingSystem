using System.ComponentModel;

namespace LoadingSystem.Model
{
	public class TextModel : INotifyPropertyChanged
	{
		private string[] arrayOfText;
		private int dataIndex;
		private string wellName;
		private string bushName;
		private string fieldName;
		private string dataSetName;

		public string[] ArrayOfText
		{
			get { return arrayOfText; }

			set
			{
				arrayOfText = value;
				OnPropertyChanged("ArrayOfText");
			}
		}

		public int DataIndex
		{
			get { return dataIndex; }

			set
			{
				dataIndex = value;
				OnPropertyChanged("DataIndex");
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

		public string BushName
		{
			get { return bushName; }

			set
			{
				bushName = value;
				OnPropertyChanged("BushName");
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

		public TextModel(int arrayLength)
		{
			ArrayOfText = new string[arrayLength];
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
