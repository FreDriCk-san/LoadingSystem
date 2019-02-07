using System;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace LoadingSystem.Model
{
    [Serializable]
	[DisplayName("Свойства файла")]
    public class PropertyGridModel
    {
        [ExpandableObject]
        [Category("Свойства")]
        [DisplayName("Общие")]
        public PropertyGridCommon PropertyGridCommon { get; set; }

		[ExpandableObject]
		[Category("Свойства")]
		[DisplayName("Тип")]
		public PropertyGridType PropertyGridType { get; set; }


		public PropertyGridModel()
        {
			PropertyGridCommon = new PropertyGridCommon();

			PropertyGridType = new PropertyGridType();
        }
    }

	public class PropertyGridCommon : INotifyPropertyChanged
	{
		private int textReadMaxLength = 1000;

		private string wellName;
		private string bushName;
		private string fieldName;
		private string dataSetName;
		private int importFrom;
		private int importTo;

		#region Data Init
		[DisplayName("Наименование скважины")]
		[Description("Наименование скважины")]
		public string WellName
		{
			get { return wellName; }

			set
			{
				wellName = value;
				OnPropertyChanged("WellName");
			}
		}


		[DisplayName("Наименование куста")]
		[Description("Наименование куста")]
		public string BushName
		{
			get { return bushName; }

			set
			{
				bushName = value;
				OnPropertyChanged("BushName");
			}
		}


		[DisplayName("Наименование месторождения")]
		[Description("Наименование месторождения")]
		public string FieldName
		{
			get { return fieldName; }

			set
			{
				fieldName = value;
				OnPropertyChanged("FieldName");
			}
		}


		[DisplayName("Наименование дата-сета")]
		[Description("Наименование дата-сета")]
		public string DataSetName
		{
			get { return dataSetName; }

			set
			{
				dataSetName = value;
				OnPropertyChanged("DataSetName");
			}
		}


		[DisplayName("Импорт из строки")]
		[Description("С какой строки вывести текстовые данные\nПРИМЕЧАНИЕ:Можно вывести не более 1000 строк")]
		public int ImportFrom
		{
			get { return importFrom; }

			set
			{
				if (!(value <= importTo - textReadMaxLength) && value >= 0)
				{
					importFrom = value;
					OnPropertyChanged("ImportFrom");
				}
			}
		}


		[DisplayName("Импорт до строки")]
		[Description("До какой строки вывести текстовые данные\nПРИМЕЧАНИЕ:Можно вывести не более 1000 строк")]
		public int ImportTo
		{
			get { return importTo; }

			set
			{
				if (!(value >= importFrom + textReadMaxLength + 1) && value >= 0)
				{
					importTo = value;
					OnPropertyChanged("ImportTo");
				}
			}
		}
		#endregion

		public PropertyGridCommon()
		{
			wellName = "";
			bushName = "";
			fieldName = "";
			dataSetName = "";
			importFrom = 0;
			importTo = 200;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}


	public class PropertyGridType : INotifyPropertyChanged
	{
		private char decimalSeparator;
		private char separator;

		#region Data Init
		[DisplayName("Разделитель десятичной дроби")]
		public char DecimalSeparator
		{
			get { return decimalSeparator; }

			set
			{
				decimalSeparator = value;
				OnPropertyChanged("DecimalSeparator");
			}
		}


		[DisplayName("Разделитель между значениями")]
		public char Separator
		{
			get { return separator; }

			set
			{
				separator = value;
				OnPropertyChanged("Separator");
			}
		}
		#endregion


		public PropertyGridType()
		{
			decimalSeparator = ' ';
			separator = ' ';
		}


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
