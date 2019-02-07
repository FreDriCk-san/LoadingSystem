using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingSystem.ViewModel
{
	public class ViewModel : INotifyPropertyChanged
	{
		private ToggleCommand fileOpenCommand;

		private Model.TextModel textModel;
		private Model.PropertyGridModel propertyGridModel;
		private List<Model.DataModel> dataModel;
		private DataTable dataGridTable;
		private DataView defaultTableView;

		private string textBoxData;
		
		public ToggleCommand FileOpenCommand
		{
			get
			{
				return fileOpenCommand ??
					(fileOpenCommand = new ToggleCommand(command =>
					{
						var openFileDialog = new OpenFileDialog();

						if (openFileDialog.ShowDialog() == true)
						{
							var textTask = Task.Run(async () =>
							{
								return await Model.FileReader.ReadAllLinesAsync(openFileDialog.FileName);
							});

							TextModel = new Model.TextModel(textTask.Result.Length);
							TextModel.ArrayOfText = textTask.Result;
							TextModel.DataIndex = Model.BusinessLogic.GetDataIndex(textModel.ArrayOfText);

							EditTextBox();

							var dataTask = Task.Run(() =>
							{
								return Model.BusinessLogic.ReadData(TextModel.ArrayOfText, TextModel.DataIndex, 200);
							});

							DataModel = dataTask.Result;

							PropertyGridModel.PropertyGridType.Separator = DataModel[0].Separator;
							PropertyGridModel.PropertyGridType.DecimalSeparator = DataModel[0].DecimalSeparator;

							EditTable();
						}


					}));
			}
		}




		#region Data Init
		public Model.TextModel TextModel
		{
			get { return textModel; }

			set
			{
				textModel = value;
				OnPropertyChanged("TextModel");
			}
		}

		public DataTable DataGridTable
		{
			get { return dataGridTable; }

			set
			{
				dataGridTable = value;
				OnPropertyChanged("DataGridTable");
			}
		}

		public DataView DefaultTableView
		{
			get { return defaultTableView; }

			set
			{
				defaultTableView = value;
				OnPropertyChanged("DefaultTableView");
			}
		}

		public List<Model.DataModel> DataModel
		{
			get { return dataModel; }

			set
			{
				dataModel = value;
				OnPropertyChanged("DataModel");
			}
		}

		public string TextBoxData
		{
			get { return textBoxData; }

			set
			{
				textBoxData = value;
				OnPropertyChanged("TextBoxData");
			}
		}

		public Model.PropertyGridModel PropertyGridModel
		{
			get { return propertyGridModel; }

			set
			{
				propertyGridModel = value;
				OnPropertyChanged("PropertyGridModel");
			}
		}
		#endregion



		public ViewModel()
		{
			PropertyGridModel = new Model.PropertyGridModel();

			PropertyGridModel.PropertyGridCommon.PropertyChanged += PropertyGridCommon_PropertyChanged;
		}

		private void PropertyGridCommon_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ImportFrom" || e.PropertyName == "ImportTo")
			{
				EditTextBox();
			}
		}

		private void EditTextBox()
		{
			var textBuilder = new StringBuilder();
			var readFrom = PropertyGridModel.PropertyGridCommon.ImportFrom;
			var readTo = PropertyGridModel.PropertyGridCommon.ImportTo;

			for (int i = readFrom; i < readTo; ++i)
			{
				textBuilder.Append($"{TextModel.ArrayOfText[i]} \n");
			}

			TextBoxData = textBuilder.ToString();
		}


		private void EditTable()
		{
			var columns = DataModel[0].ListOfNumbers.Count;
			DataGridTable = new Model.DataGridModel().DataGridTable;
			DefaultTableView = new DataView();
			DataGridTable.Columns.Clear();
			DataGridTable.BeginLoadData();

			for (int i = 1; i <= columns; ++i)
			{
				DataGridTable.Columns.Add(i.ToString());
			}

			var content = new object[columns];

			for (int i = 0; i < DataModel.Count; ++i)
			{
				content = DataModel[i].ListOfNumbers.Cast<object>().ToArray();
				DataGridTable.Rows.Add(content);
			}

			DataGridTable.EndLoadData();
			DefaultTableView = DataGridTable.DefaultView;
		}



		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}