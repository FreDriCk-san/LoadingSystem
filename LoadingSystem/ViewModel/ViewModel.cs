﻿using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LoadingSystem.ViewModel
{
	public class ViewModel : INotifyPropertyChanged
	{
        private ConcurrentBag<Task> tasks;
		private FileInfo fileInfo;
		private int countOfRows;

		private ToggleCommand fileOpenCommand;
		private ToggleCommand changeTextBoxCommand;
		private ToggleCommand saveToExcelFormat;
		private ToggleCommand changeTableCommand;
        private ToggleCommand cancelLoading;

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
        private CancellationTokenSource tokenSource;
        private CancellationToken cancellationToken;

		private bool loadingGridVisible;

		
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
						InitCancelToken();

						var readTo = PropertyGridModel.OutputDescription.ImportTo;

						var textTask = Task.Run(async () =>
						{
							return await Model.FileReader.ReadLinesAsync(fileInfo.FullName, readTo, cancellationToken);
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
					(saveToExcelFormat = new ToggleCommand(async command =>
					{
						if (DataGridTable.Columns.Count > 0)
						{
							using (var excelPackage = new OfficeOpenXml.ExcelPackage())
							{
								var isRead = await Model.ConvertToExcel.ReadData(DataGridTable, excelPackage);

								using (var dialog = new System.Windows.Forms.SaveFileDialog())
								{
									// Set default extension types of file
									dialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";

									if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
									{
										var isSaved = await Model.ConvertToExcel.SaveAsExcel(dialog.FileName, excelPackage);

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



        public ToggleCommand CancelLoading
        {
            get
            {
                return cancelLoading ??
                    (cancelLoading = new ToggleCommand(command =>
                    {
                        tokenSource.Cancel();
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

		public bool LoadingGridVisible
		{
			get { return loadingGridVisible; }

			set
			{
				loadingGridVisible = value;
				OnPropertyChanged("LoadingGridVisible");
			}
		}
		#endregion



		public ViewModel()
		{
            tasks = new ConcurrentBag<Task>();

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
			LoadingGridVisible = true;
            InitCancelToken();

			var process = Task.Factory.StartNew(() =>
			{

				ProgressValue = 0;

				fileInfo = new FileInfo(path);

				canChangeNullValue = true;

				// If excel file 2007-2010
				if (fileInfo.FullName.EndsWith(".xlsx"))
				{
					Model.FileReader.ReadAsXLSX(fileInfo.FullName, cancellationToken);
					ProgressValue += 3;
				}
				// If excel file 2003-2007
				else if (fileInfo.FullName.EndsWith(".xls"))
				{
					DataModel = Model.FileReader.ReadAsXLS(fileInfo.FullName, cancellationToken);
					ProgressValue += 3;
				}
				// If text file (or .csv)
				else
				{
					var textTasks = Task.Run(async () =>
					{
						return await Model.FileReader.ReadLinesAsync(fileInfo.FullName, 100, cancellationToken);
					});
					tasks.Add(textTasks);
					ProgressValue++;


					EditTextBox(textTasks.Result, 100);
					ProgressValue++;


					var dataTask = Task.Run(async () =>
					{
						return await Model.FileReader.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
					});
					tasks.Add(dataTask);
					ProgressValue++;

					DataModel = dataTask.Result;
				}

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

			}, cancellationToken).ContinueWith(task =>
			{
				LoadingGridVisible = false;
			});

			tasks.Add(process);
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


        private void InitCancelToken()
        {
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}