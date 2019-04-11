using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LoadingSystem.ViewModel
{
	public class ViewModel : INotifyPropertyChanged
	{
        public int countOfRows;

        private ConcurrentBag<Task> tasks;
		private FileInfo fileInfo;
		private int currentTab;
		private List<int> listOfCheckedValues;


		private ToggleCommand fileOpenCommand;
		private ToggleCommand saveToExcelFormat;
        private ToggleCommand cancelLoading;

        private Model.PropertiesModel propertiesModel;
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
		private string fullFilePath;

		private ObservableCollection<TabItem> tabCollection;

		private bool createIndexCurve = false;

        private string tbImportTo;
        private string tbTableImportFrom;
        private string tbTableImportTo;

		
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



		public ToggleCommand SaveToExcelFormat
		{
			get
			{
				return saveToExcelFormat ??
					(saveToExcelFormat = new ToggleCommand(async command =>
					{
						if (DataGridTable.Columns.Count > 0)
						{
							ProgressValue = 0;
							LoadingGridVisible = true;

							using (var excelPackage = new OfficeOpenXml.ExcelPackage())
							{
								var tableForExport = GetTableForExport(DataGridTable);

								if (null == tableForExport)
								{
									MessageBox.Show("Импортируемые значения не выбраны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
								}
								else
								{
									var isRead = await Model.ConvertToExcel.ReadData(tableForExport, excelPackage, fileInfo.Name, DataModel.ListOfCurveNames);

									using (var dialog = new System.Windows.Forms.SaveFileDialog())
									{
										// Set default extension types of file
										dialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";

										if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
										{
											var isSaved = await Model.ConvertToExcel.SaveAsExcel(dialog.FileName, excelPackage);
										}
									}
								}
							}
						}
						else
						{
							MessageBox.Show("Текущая таблица пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
						}

						ProgressValue += 5;
						LoadingGridVisible = false;
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

		public Model.PropertiesModel PropertiesModel
        {
            get { return propertiesModel; }

            set
            {
                propertiesModel = value;
                OnPropertyChanged("PropertiesModel");
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
                    PreEditTable();
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

		public string FullFilePath
		{
			get { return fullFilePath; }

			set
			{
				fullFilePath = value;
				OnPropertyChanged("FullFilePath");
			}
		}

		public ObservableCollection<TabItem> TabCollection
		{
			get { return tabCollection; }

			set
			{
				tabCollection = value;
				OnPropertyChanged("TabCollection");
			}
		}

        public string TbImportTo
        {
            get { return tbImportTo; }

            set
            {
                tbImportTo = value;
                OnPropertyChanged("TbImportTo");
            }
        }

        public string TbTableImportFrom
        {
            get { return tbTableImportFrom; }

            set
            {
                tbTableImportFrom = value;
                OnPropertyChanged("TbTableImportFrom");
            }
        }

        public string TbTableImportTo
        {
            get { return tbTableImportTo; }

            set
            {
                tbTableImportTo = value;
                OnPropertyChanged("TbTableImportTo");
            }
        }
		#endregion



		public ViewModel()
		{
            tasks = new ConcurrentBag<Task>();
			listOfCheckedValues = new List<int>();

            PropertiesModel = new Model.PropertiesModel();
			DataModel = new Model.DataModel();

			CollectionOfNull = new ObservableCollection<double>
			{
				-999.00, -9999.25
			};

			TabCollection = new ObservableCollection<TabItem>();
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

			if (createIndexCurve)
			{
				DataGridTable.Columns.Add("Индексная кривая");

				for (int i = 1; i <= columns; ++i)
				{
					DataGridTable.Columns.Add(i.ToString());
				}

				var content = new object[columns + 1];

				for (int i = fromRow, t = 1; i < toRow; ++i, ++t)
				{
					content[0] = t;

					for (int j = 0; j < columns; ++j)
					{
						var currentValue = DataModel.ArrayOfNumbers[i][j];

						if (currentValue == double.MinValue || currentValue == CurrentNull)
						{
							content[j + 1] = string.Empty;
						}
						else
						{
							content[j + 1] = currentValue;
						}
					}

					DataGridTable.Rows.Add(content);
				}

				DataGridTable.Columns[0].ReadOnly = true;
			}
			else
			{
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

						if (currentValue == double.MinValue || currentValue == CurrentNull)
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
			}

			DataGridTable.EndLoadData();

			Application.Current.Dispatcher.Invoke(() =>
			{
				DefaultTableView = DataGridTable.DefaultView;
			});
		}



        public async void ProcessIncomingFile(string path)
        {
			LoadingGridVisible = true;
			FullFilePath = path;
            InitCancelToken();

            var process = new Task(() =>
             {

                 ProgressValue = 0;

                 fileInfo = new FileInfo(path);

                 canChangeNullValue = true;

                 // If excel file 2007+ (BIFF 12)
                 if (fileInfo.FullName.EndsWith(".xlsx"))
                 {
                     currentTab = 1;

                     var text = Task.Run(async () =>
                     {
                         return await Model.FileReader.ReadLinesFromXLSX(fileInfo.FullName, 100, 1, cancellationToken);
                     });
                     ProgressValue++;

                     if (null == text.Result)
                     {
                         MessageBox.Show("Данные недоступны. Возможно, файл уже где-то открыт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                         Thread.CurrentThread.Abort();
                     }

                     EditTextBox(text.Result, 100);
                     ProgressValue++;

                     DataModel = Model.FileReader.ReadAsXLSX(fileInfo.FullName, 1, cancellationToken);
                     ProgressValue++;
                 }
                 // TO DO: Make normal verification
                 else if (fileInfo.FullName.EndsWith(".XLS"))
                 {
                     MessageBox.Show("Недопустимый формат файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                     Thread.CurrentThread.Abort();
                 }
                 // If excel file 1997-2003 (BIFF 8)
                 else if (fileInfo.FullName.EndsWith(".xls"))
                 {
                     currentTab = 0;

                     var text = Task.Run(async () =>
                     {
                         return await Model.FileReader.ReadLinesFromXLS(fileInfo.FullName, 100, 0, cancellationToken);
                     });
                     ProgressValue++;

                     if (null == text.Result)
                     {
                         MessageBox.Show("Данные недоступны. Возможно, файл уже где-то открыт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                         Thread.CurrentThread.Abort();
                     }

                     EditTextBox(text.Result, 100);
                     ProgressValue++;

                     DataModel = Model.FileReader.ReadAsXLS(fileInfo.FullName, 0, cancellationToken);
                     ProgressValue++;
                 }
                 // If text file
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

                 TbImportTo = "100";
                 TbTableImportFrom = "0";
                 TbTableImportTo = countOfRows.ToString();

                 InitTabs(DataModel.ArrayOfWorkSheetsName);

                 DepthValue = SearchDepthColumn(DataModel.ArrayOfNumbers, 0, countOfRows);
                 ProgressValue++;


                 SetPropertiesToView();
                 ProgressValue++;


                 CheckDataNullValue();
                 ProgressValue++;


                 PreEditTable();
                 ProgressValue++;

             }, cancellationToken);

            process.Start();
            await process;
            process.Dispose();

            LoadingGridVisible = false;
        }



		public async void UpdatedTab(int tabNum)
		{
			currentTab = tabNum;
            listOfCheckedValues.Clear();

            var process = new Task(() =>
            {
                // If excel file 2007+ (BIFF 12)
                if (fileInfo.FullName.EndsWith(".xlsx"))
                {
                    var text = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadLinesFromXLSX(fileInfo.FullName, 100, tabNum, cancellationToken);
                    });

                    if (null == text.Result)
                    {
                        MessageBox.Show("Данные недоступны. Возможно, файл уже где-то открыт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.CurrentThread.Abort();
                    }

                    EditTextBox(text.Result, 100);

                    DataModel = Model.FileReader.ReadAsXLSX(fileInfo.FullName, tabNum, cancellationToken);
                }
                // TO DO: Make normal verification
                else if (fileInfo.FullName.EndsWith(".XLS"))
                {
                    MessageBox.Show("Недопустимый формат файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Thread.CurrentThread.Abort();
                }
                // If excel file 1997-2003 (BIFF 8)
                else if (fileInfo.FullName.EndsWith(".xls"))
                {
                    var text = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadLinesFromXLS(fileInfo.FullName, 100, tabNum, cancellationToken);
                    });

                    if (null == text.Result)
                    {
                        MessageBox.Show("Данные недоступны. Возможно, файл уже где-то открыт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Thread.CurrentThread.Abort();
                    }

                    EditTextBox(text.Result, 100);

                    DataModel = Model.FileReader.ReadAsXLS(fileInfo.FullName, tabNum, cancellationToken);
                }
                // If text file
                else
                {
                    var textTasks = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadLinesAsync(fileInfo.FullName, 100, cancellationToken);
                    });
                    tasks.Add(textTasks);


                    EditTextBox(textTasks.Result, 100);


                    var dataTask = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
                    });
                    tasks.Add(dataTask);

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

                TbImportTo = "100";
                TbTableImportFrom = "0";
                TbTableImportTo = countOfRows.ToString();


                DepthValue = SearchDepthColumn(DataModel.ArrayOfNumbers, 0, countOfRows);


                SetPropertiesToView();


                CheckDataNullValue();


                PreEditTable();

            }, cancellationToken);

            process.Start();
            await process;
            process.Dispose();

            LoadingGridVisible = false;
        }



        private void SetPropertiesToView()
		{
            PropertiesModel.TbInfoStartsFrom = DataModel.DataStartsFrom.ToString();
            PropertiesModel.TbDataSetName = DataModel.DataSetName;
            PropertiesModel.TbFieldName = DataModel.FieldName;
            PropertiesModel.TbWellName = DataModel.WellName;
            PropertiesModel.TbDecimalSeparator = DataModel.DecimalSeparator.ToString();
            PropertiesModel.TbSeparator = DataModel.Separator.ToString();
		}



		private int SearchDepthColumn(double[][] arrayOfNumbers, int fromRow, int toRow)
		{
			if (null != arrayOfNumbers[0])
			{
				var length = arrayOfNumbers[0].Length;

				for (int i = 0; i < length; ++i)
				{
					var min = 0;
					var max = 0;

					for (int j = fromRow; j < toRow - 1; ++j)
					{
						var currentValue = arrayOfNumbers[j][i];
						var nextValue = arrayOfNumbers[j + 1][i];

						if (currentValue < nextValue)
						{
							max++;
						}
						else if (currentValue > nextValue)
						{
							min++;
						}
					}

					var currentCountOfRows = countOfRows - (fromRow + (countOfRows - toRow) + 1);

					if (max == currentCountOfRows
						|| min == currentCountOfRows)
					{
						createIndexCurve = false;
						return i;
					}
				}
			}

			createIndexCurve = true;
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



		private void InitTabs(string[] arrayOfSheetNames)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				TabCollection.Clear();
			});

			var localIndex = 0;

			if (currentTab > 0)
			{
				for (int i = 1; i <= DataModel.CountOfWorkSpaces; ++i, ++localIndex)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						var tab = new TabItem
						{
							Header = $"{i.ToString()}. {arrayOfSheetNames[localIndex]}"
						};
						tab.MouseLeftButtonUp += Tab_MouseLeftButtonUp;

						TabCollection.Add(tab);
					});
				}
			}
			else
			{
				for (int i = 0; i < DataModel.CountOfWorkSpaces; ++i, ++localIndex)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						var tab = new TabItem
						{
							Header = $"{i.ToString()}. {arrayOfSheetNames[localIndex]}"
						};
						tab.MouseLeftButtonUp += Tab_MouseLeftButtonUp;

						TabCollection.Add(tab);
					});
				}
			}
		}



		private void Tab_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var currentTab = sender as TabItem;

			var header = currentTab.Header.ToString();
			var builder = new StringBuilder();

			foreach(var character in header)
			{
				if (character == '.')
				{
					break;
				}

				builder.Append(character);
			}

			var currentTabIndex = Int32.Parse(builder.ToString());

			UpdatedTab(currentTabIndex);
		}



		public void SetImportIndex(Util.CustomCheckBox customCheckBox)
		{
			var checkBox = customCheckBox;

			if (checkBox.IsChecked == true)
			{
				var itemIsPresent = false;

				for (int i = 0; i < listOfCheckedValues.Count; ++i)
				{
					if (listOfCheckedValues[i] == checkBox.Index)
					{
						itemIsPresent = true;
					}
				}

				if (!itemIsPresent)
				{
					listOfCheckedValues.Add(checkBox.Index);
				}
			}
			else
			{
				for (int i = 0; i < listOfCheckedValues.Count; ++i)
				{
					if (listOfCheckedValues[i] == checkBox.Index)
					{
						listOfCheckedValues.RemoveAt(i);
					}
				}
			}
		}



        public async void ChangeTextBoxAsync(int readTo)
        {
            InitCancelToken();

            if (readTo < 0 || readTo > 1000)
            {
                readTo = 100;
                TbImportTo = "100";
            }

            if (fileInfo.FullName.EndsWith(".xlsx"))
            {
                var text = await Model.FileReader.ReadLinesFromXLSX(fileInfo.FullName, readTo, currentTab, cancellationToken);

                EditTextBox(text, readTo);
            }
            else if (fileInfo.FullName.EndsWith(".xls"))
            {
                var text = await Model.FileReader.ReadLinesFromXLS(fileInfo.FullName, readTo, currentTab, cancellationToken);

                EditTextBox(text, readTo);
            }
            else
            {
                var textTask = Task.Run(async () =>
                {
                    return await Model.FileReader.ReadLinesAsync(fileInfo.FullName, readTo, cancellationToken);
                });

                EditTextBox(textTask.Result, readTo);
            }
        }



        public void ChangeTable(int importFrom, int importTo)
        {
            if (importTo > countOfRows || importTo < 0)
            {
                TbTableImportTo = countOfRows.ToString();
                importTo = countOfRows;
            }
            else if (importFrom > countOfRows || importFrom < 0)
            {
                TbTableImportFrom = "0";
                importFrom = 0;
            }

            DepthValue = SearchDepthColumn(DataModel.ArrayOfNumbers, importFrom, importTo);
            EditTable(importFrom, importTo);
        }



		private DataTable GetTableForExport(DataTable dataTable)
		{
			if (listOfCheckedValues.Count == 0)
			{
				return null;
			}

			// Sort list by ascend before using CheckBox positions
			var sortedListOfChecks = Util.DataSorting.MergeSort(listOfCheckedValues);

			var tableForExport = new DataTable();
			var rowCount = dataTable.Rows.Count;
			var listCount = sortedListOfChecks.Count;

			tableForExport.BeginLoadData();

			// Add columns
			for (int i = 0; i < listCount; ++i)
			{
				var currentIndex = sortedListOfChecks[i];

				tableForExport.Columns.Add(currentIndex.ToString());
			}

			// Add rows
			for (int i = 0; i < rowCount; ++i)
			{
				var data = new object[listCount];

				for (int j = 0; j < listCount; ++j)
				{
					var currentIndex = sortedListOfChecks[j];

					data[j] = dataTable.Rows[i][currentIndex];
				}

				tableForExport.Rows.Add(data);
			}

			tableForExport.EndLoadData();

			return tableForExport;
		}



        protected void PreEditTable()
        {
            if (int.TryParse(TbTableImportFrom, out var importFrom) && int.TryParse(TbTableImportTo, out var importTo))
            {
                EditTable(importFrom, importTo);
            }
            else
            {
                MessageBox.Show("Задайте значения импорта данных таблицы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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