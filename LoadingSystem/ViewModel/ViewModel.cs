using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LoadingSystem.ViewModel
{
	public class ViewModel : INotifyPropertyChanged
	{
		private string filePath;

		private ToggleCommand fileOpenCommand;
		private ToggleCommand changeTextBoxCommand;
		private ToggleCommand saveToExcelFormat;

		private Model.PropertyGridModel propertyGridModel;
		private Model.DataModel dataModel;
		private DataTable dataGridTable;
		private DataView defaultTableView;

		private string textBoxData;

		private ObservableCollection<double> collectionOfNull;
		private double currentNull;
		private bool canChangeNullValue = false;
		
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

							canChangeNullValue = true;

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

							SetPropertiesToPropertyGrid();

							CheckDataNullValue();

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


		public ToggleCommand SaveToExcelFormat
		{
			get
			{
				return saveToExcelFormat ??
					(saveToExcelFormat = new ToggleCommand(command =>
					{
						if (DataGridTable.Columns.Count > 0)
						{
							using (var excelPackage = new ExcelPackage())
							{
								var workSheets = excelPackage.Workbook.Worksheets.Add("ExampleDataTable");

								// TO DO: Set style or format for output
								workSheets.Cells["A1"].LoadFromDataTable(DataGridTable, true, OfficeOpenXml.Table.TableStyles.Medium9);

								using (var dialog = new System.Windows.Forms.SaveFileDialog())
								{
									// Set default extension types of file
									dialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";

									if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
									{
										var path = new FileInfo(dialog.FileName);

										excelPackage.SaveAs(path);
									}
								}
							}
						}
						else
						{
							MessageBox.Show("Текущая таблица пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
						}
						

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

		public ObservableCollection<double> CollectionOfNull
		{
			get { return collectionOfNull; }

			set
			{
				collectionOfNull = value;
				OnPropertyChanged("CollectionOfNull");
			}
		}

		public double CurrentNull
		{
			get { return currentNull; }

			set
			{
				currentNull = value;
				OnPropertyChanged("CurrentNull");

				if (canChangeNullValue)
				{
					EditTable();
				}
			}
		}
		#endregion



		public ViewModel()
		{
			PropertyGridModel = new Model.PropertyGridModel();
			DataModel = new Model.DataModel();

			CollectionOfNull = new ObservableCollection<double>
			{
				-999.00, -9999.25
			};

			CurrentNull = -999.00;
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

			var existingElements = 0;

			for (int i = 0; i < DataModel.ArrayOfNumbers.Length; ++i)
			{
				if (null == DataModel.ArrayOfNumbers[i])
				{
					existingElements = i;
					break;
				}
			}

			for (int i = 0; i < existingElements; ++i)
			{
				for (int j = 0; j < columns; ++j)
				{
					var currentValue = DataModel.ArrayOfNumbers[i][j];
					// Check for count of NaN values
					//var tmp = DataModel.ArrayOfNumbers[i][j];

					//if (double.IsNaN(tmp))
					//{

					//}

					if (currentValue == CurrentNull)
					{
						content[j] = double.NaN;
					}
					else
					{
						content[j] = currentValue;
					}
				}
				DataGridTable.Rows.Add(content);
			}

			DataGridTable.EndLoadData();
			DefaultTableView = DataGridTable.DefaultView;
		}



		private void SetPropertiesToPropertyGrid()
		{
			PropertyGridModel.PropertyGridType.Separator = DataModel.Separator;
			PropertyGridModel.PropertyGridType.DecimalSeparator = DataModel.DecimalSeparator;

			PropertyGridModel.OutputDescription.DataStartsFrom = DataModel.DataStartsFrom;

			PropertyGridModel.PropertyGridCommon.WellName = DataModel.WellName;
			PropertyGridModel.PropertyGridCommon.FieldName = DataModel.FieldName;
			PropertyGridModel.PropertyGridCommon.DataSetName = DataModel.DataSetName;
			PropertyGridModel.PropertyGridCommon.BushName = DataModel.BushName;
		}



		private void CheckDataNullValue()
		{
			var nullValue = DataModel.NullValue;

			if (nullValue != default(double))
			{
				var isPresent = false;

				foreach (var item in CollectionOfNull)
				{
					if (nullValue == item)
					{
						isPresent = true;
					}
				}

				if (!isPresent)
				{
					CollectionOfNull.Add(nullValue);
				}

				CurrentNull = nullValue;
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}