using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingSystem.ViewModel
{
	public class ViewModel : INotifyPropertyChanged
	{
		private string filePath;

		private ToggleCommand fileOpenCommand;
		private ToggleCommand changeTextBoxCommand;

		private Model.PropertyGridModel propertyGridModel;
		private Model.DataModel dataModel;
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
							filePath = openFileDialog.FileName;

							var textTasks = Task.Run(async () =>
							{
								return await Model.FileReader.ReadLinesAsync(filePath, 100);
							});

							EditTextBox(textTasks.Result, 100);

							var dataTask = Task.Run(async () =>
							{
								return await Model.FileReader.ReadAllLinesAsync(filePath);
							});

							DataModel = dataTask.Result;

							PropertyGridModel.PropertyGridType.Separator = DataModel.Separator;
							PropertyGridModel.PropertyGridType.DecimalSeparator = DataModel.DecimalSeparator;
                            PropertyGridModel.OutputDescription.DataStartsFrom = DataModel.DataStartsFrom;

							EditTable();
						}


					}));
			}
		}


		public ToggleCommand ChangeTextBoxCommand
		{
			get
			{
				return changeTextBoxCommand ??
					(changeTextBoxCommand = new ToggleCommand(command =>
					{
						var readTo = PropertyGridModel.OutputDescription.ImportTo;

						var textTask = Task.Run(async () =>
						{
							return await Model.FileReader.ReadLinesAsync(filePath, readTo);
						});

						EditTextBox(textTask.Result, readTo);
					}));
			}
		}



		#region Data Init
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

		public Model.DataModel DataModel
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
            DataModel = new Model.DataModel();
		}



		private void EditTextBox(string[] data, int readTo)
		{
			var textBuilder = new StringBuilder();

			for (int i = 0; i < readTo; ++i)
			{
				textBuilder.Append($"{data[i]} \n");
			}

			TextBoxData = textBuilder.ToString();
		}



		private void EditTable()
		{
			var columns = DataModel.ColumnCount;
			DataGridTable = new Model.DataGridModel().DataGridTable;
			DefaultTableView = new DataView();
			DataGridTable.Columns.Clear();
			DataGridTable.BeginLoadData();

			for (int i = 1; i <= columns; ++i)
			{
				DataGridTable.Columns.Add(i.ToString());
			}

			var content = new object[columns];

			var existingElements = DataModel.ArrayOfNumbers.Where(c => c != null).ToArray();

			for (int i = 0; i < existingElements.Length; ++i)
			{
				for (int j = 0; j < columns; ++j)
				{
					content[j] = DataModel.ArrayOfNumbers[i][j];
				}
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