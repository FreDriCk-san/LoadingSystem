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
		private int countOfRows;

		private ToggleCommand fileOpenCommand;
		private ToggleCommand changeTextBoxCommand;
		private ToggleCommand saveToExcelFormat;
		private ToggleCommand saveToHTMLFormat;
		private ToggleCommand changeTableCommand;

		private Model.PropertyGridModel propertyGridModel;
		private Model.DataModel dataModel;
		private DataTable dataGridTable;
		private DataView defaultTableView;

		private string textBoxData;

		private ObservableCollection<double> collectionOfNull;
		private double currentNull = -999.00;
		private bool canChangeNullValue = false;

		private int depthValue;
		private double progressValue;
		
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
							// TO DO: Set progressbar
							ProcessIncomingFile(openFileDialog.FileName);
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
								var workSheets = excelPackage.Workbook.Worksheets.Add("DataTable");

								// TO DO: Set style or format for output
								workSheets.Cells["A1"].LoadFromDataTable(DataGridTable, true, OfficeOpenXml.Table.TableStyles.Medium9);

								using (var dialog = new System.Windows.Forms.SaveFileDialog())
								{
									// Set default extension types of file
									dialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";

									if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
									{
										var path = new FileInfo(dialog.FileName);

										excelPackage.SaveAs(path);

										MessageBox.Show("Преобразовано");						
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



		public ToggleCommand SaveToHTMLFormat
		{
			get
			{
				return saveToHTMLFormat ??
					(saveToHTMLFormat = new ToggleCommand(command =>
					{
						if (DataGridTable.Columns.Count > 0)
						{
							using (var dialog = new System.Windows.Forms.SaveFileDialog())
							{
								// Set default extension types of file
								dialog.Filter = "HTML (*.html)|*.html";

								if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
								{
									var saveTask = Task.Run(async () =>
									{
										await Model.ConvertToHTML.ProceedDataTableAsync(DataGridTable, DataModel.ColumnCount, dialog.FileName);
									});

									saveTask.ContinueWith(task => {
										MessageBox.Show("Преобразовано в HTML");
										saveTask.Dispose();
									});

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



		public ToggleCommand ChangeTableCommand
		{
			get
			{
				return changeTableCommand ??
					(changeTableCommand = new ToggleCommand(command =>
					{
						var readFrom = PropertyGridModel.OutputDescription.ReadFromRow;
						var readTo = PropertyGridModel.OutputDescription.ReadToRow;

						if (readTo > countOfRows)
						{
							PropertyGridModel.OutputDescription.ReadToRow = countOfRows;
							readTo = countOfRows;
						}

						EditTable(readFrom, readTo);
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
					EditTable(PropertyGridModel.OutputDescription.ReadFromRow, PropertyGridModel.OutputDescription.ReadToRow);
				}
			}
		}

		public int DepthValue
		{
			get { return depthValue; }

			set
			{
				depthValue = value;
				OnPropertyChanged("DepthValue");
			}
		}

		public double ProgressValue
		{
			get { return progressValue; }

			set
			{
				progressValue = value;
				OnPropertyChanged("ProgressValue");
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



		private void EditTable(int fromRow, int toRow)
		{
			var columns = DataModel.ColumnCount;

			if (DataGridTable != null)
			{
				DataGridTable.Clear();
			}

			DataGridTable = new Model.DataGridModel().DataGridTable;
			DefaultTableView = new DataView();
			DataGridTable.Columns.Clear();
			DataGridTable.BeginLoadData();

			for (int i = 1; i <= columns; ++i)
			{
				DataGridTable.Columns.Add(i.ToString());
			}

			var content = new object[columns];

			for (int i = fromRow; i < toRow; ++i)
			{
				for (int j = 0; j < columns; ++j)
				{
					var currentValue = DataModel.ArrayOfNumbers[i][j];

					if (currentValue == CurrentNull)
					{
						content[j] = double.NaN;
					}
					else if (currentValue == double.MinValue)
					{
						content[j] = string.Empty;
					}
					else
					{
						content[j] = currentValue;
					}
				}
				DataGridTable.Rows.Add(content);
			}

			DataGridTable.EndLoadData();

			Application.Current.Dispatcher.Invoke(() =>
			{
				DefaultTableView = DataGridTable.DefaultView;
			});
			
		}



		public void ProcessIncomingFile(string path)
		{

			Task.Factory.StartNew(() =>
			{
				ProgressValue = 0;

				filePath = path;

				canChangeNullValue = true;

				var textTasks = Task.Run(async () =>
				{
					return await Model.FileReader.ReadLinesAsync(filePath, 100);
				});
				ProgressValue++;

				EditTextBox(textTasks.Result, 100);
				ProgressValue++;

				var dataTask = Task.Run(async () =>
				{
					return await Model.FileReader.ReadAllLinesAsync(filePath);
				});
				ProgressValue++;

				DataModel = dataTask.Result;

				for (int i = 0; i < DataModel.ArrayOfNumbers.Length; ++i)
				{
					if (null == DataModel.ArrayOfNumbers[i])
					{
						countOfRows = i;
						break;
					}
				}
				ProgressValue++;

				DepthValue = SearchDepthColumn(DataModel.ArrayOfNumbers);
				ProgressValue++;

				SetPropertiesToPropertyGrid();
				ProgressValue++;

				CheckDataNullValue();
				ProgressValue++;

				EditTable(PropertyGridModel.OutputDescription.ReadFromRow, PropertyGridModel.OutputDescription.ReadToRow);
				ProgressValue++;
			});
			
			
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

			PropertyGridModel.OutputDescription.ReadToRow = countOfRows;
		}



		private int SearchDepthColumn(double[][] arrayOfNumbers)
		{
			var length = arrayOfNumbers[0].Length;

			for (int i = 0; i < length; ++i)
			{
				var min = 0;
				var max = 0;

				for (int j = 0; j < countOfRows - 1; ++j)
				{
					var currentValue = arrayOfNumbers[j][i];
					var nextValue = arrayOfNumbers[j + 1][i];

					if (currentValue == double.MinValue || currentValue == double.NaN
						|| nextValue == double.MinValue || nextValue == double.NaN)
					{
						min++;
						max++;
					}
					else if (currentValue <= nextValue)
					{
						max++;
					}
					else if (currentValue >= nextValue)
					{
						min++;
					}
				}

				if (max == countOfRows - 1 || min == countOfRows - 1)
				{
					return i;
				}
			}

			return 0;
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
					Application.Current.Dispatcher.Invoke(() =>
					{
						CollectionOfNull.Add(nullValue);
					});
					
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