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


        #region Описание
        /// <summary>
        /// Команда открытия файла
        /// </summary>
        #endregion
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


        #region Описание
        /// <summary>
        /// Команда сохранения в Excel формат
        /// </summary>
        #endregion
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


        #region Описание
        /// <summary>
        /// Команда отмены текущего действия
        /// </summary>
        #endregion
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
			listOfCheckedValues = new List<int>();

            PropertiesModel = new Model.PropertiesModel();
			DataModel = new Model.DataModel();

			CollectionOfNull = new ObservableCollection<double>
			{
				-999.00, -9999.25
			};

			TabCollection = new ObservableCollection<TabItem>();
		}


        #region Описание
        /// <summary>
        /// Перепечатывание текста в блоке вывода содержимого входных данных
        /// </summary>
        /// <param name="data">Массив строк входных данных</param>
        /// <param name="readTo">До какой строки считывать</param>
        #endregion
        private void EditTextBox(string[] data, int readTo)
		{
			var textBuilder = new StringBuilder();

			for (int i = 0; i < readTo; ++i)
			{
				textBuilder.Append($"{data[i]} \n");
			}

			TextBoxData = textBuilder.ToString();
		}


        #region Описание
        /// <summary>
        /// Инициализация таблицы и её дальнейшее заполнение полученными данными
        /// </summary>
        /// <param name="fromRow">С какой строки (ячейки)</param>
        /// <param name="toRow">По какую строку (ячейку)</param>
        #endregion
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


        #region Описание
        /// <summary>
        /// Первоначальная обработка входных данных
        /// </summary>
        /// <param name="path">Путь файла входных данных</param>
        #endregion
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
                        return;
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
                    return;
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
                        return;
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
                    ProgressValue++;


                    EditTextBox(textTasks.Result, 100);
                    ProgressValue++;


                    var dataTask = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
                    });
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

            try
            {
                process.Start();
                await process;
            }
            finally
            {
                process.Dispose();
            }

            LoadingGridVisible = false;
        }


        #region Описание
        /// <summary>
        /// Обновление содержимого компоненты при смене вкладки (рабочей области, книги...)
        /// </summary>
        /// <param name="tabNum">Номер вкладки</param>
        #endregion
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
                        return;
                    }

                    EditTextBox(text.Result, 100);

                    DataModel = Model.FileReader.ReadAsXLSX(fileInfo.FullName, tabNum, cancellationToken);
                }
                // TO DO: Make normal verification
                else if (fileInfo.FullName.EndsWith(".XLS"))
                {
                    MessageBox.Show("Недопустимый формат файла!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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
                        return;
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

                    EditTextBox(textTasks.Result, 100);


                    var dataTask = Task.Run(async () =>
                    {
                        return await Model.FileReader.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
                    });

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

            try
            {
                process.Start();
                await process;
            }
            finally
            {
                process.Dispose();
            }

            LoadingGridVisible = false;
        }


        #region Описание
        /// <summary>
        /// Установка свойств в классе "PropertiesModel"
        /// </summary>
        #endregion
        private void SetPropertiesToView()
		{
            PropertiesModel.TbInfoStartsFrom = DataModel.DataStartsFrom.ToString();
            PropertiesModel.TbDataSetName = DataModel.DataSetName;
            PropertiesModel.TbFieldName = DataModel.FieldName;
            PropertiesModel.TbWellName = DataModel.WellName;
            PropertiesModel.TbDecimalSeparator = DataModel.DecimalSeparator.ToString();
            PropertiesModel.TbSeparator = DataModel.Separator.ToString();
		}


        #region Описание
        /// <summary>
        /// Поиск кривой, отвечающей за "Глубину" (DEPTH).
        /// </summary>
        /// <param name="arrayOfNumbers">Массив входных (обработанных) данных</param>
        /// <param name="fromRow">С какой строки таблицы искать</param>
        /// <param name="toRow">До какой строки таблицы искать</param>
        /// <returns>
        /// Возвращает номер столбца, содержащую кривую глубины. 
        /// Если таковую не удалось найти, флагу создания "Индексной кривой" присваивается значение true.
        /// </returns>
        #endregion
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


        #region Описание
        /// <summary>
        /// Определение null-значения и его последующий поиск в существующем списке "CollectionOfNull".
        /// Если таковой найти не удаётся, происходит добавление в этот список.
        /// </summary>
        #endregion
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


        #region Описание
        /// <summary>
        /// Инициализация токена отмены
        /// </summary>
        #endregion
        private void InitCancelToken()
        {
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
        }


        #region Описание
        /// <summary>
        /// Инициализация вкладок (рабочих областей, книг...).
        /// Каждой созданной вкладке присваивается локальный ивент "Tab_MouseLeftButton".
        /// </summary>
        /// <param name="arrayOfSheetNames">Массив имён вкладок. Должен быть результатом считывания данных входного файла.</param>
        #endregion
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


        #region Описание
        /// <summary>
        /// При нажатии на вкладку происходит перепечатывание содержимого компоненты.
        /// Исходя из названия, будет загружена та или иная рабочая область.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #endregion
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


        #region Описание
        /// <summary>
        /// Установка импортируемых столбцов.
        /// При клике на элемент "CheckBox" происходит добавление\удаление элемента во внутреннем списке.
        /// При вызове импорта, производится выбор определённых столбцов (из элементов внутреннего списка).
        /// </summary>
        /// <param name="customCheckBox">"CheckBox", который необходимо обработать</param>
        #endregion
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


        #region Описание
        /// <summary>
        /// Асинхронное перепечатываение содержимого блока вывода входных данных.
        /// </summary>
        /// <param name="readTo">До какой строки перепечатывать текст</param>
        #endregion
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


        #region Описание
        /// <summary>
        /// Перепечатывание таблицы (с какой строки по какую строку)
        /// </summary>
        /// <param name="importFrom">С какой ячейки (строки) выводить данные</param>
        /// <param name="importTo">До какой ячейки (строки) выводить данные</param>
        #endregion
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


        #region Описание
        /// <summary>
        /// Инициализация импортируемой таблицы, исходя из данных, полученных при обработке
        /// и локальном списке импортируемых столбцов.
        /// </summary>
        /// <param name="dataTable">Таблица, полученная при обработке входящих данных</param>
        /// <returns>Возвращает таблицу, которую необходимо перевести во внутренний формат</returns>
        #endregion
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


        #region Описание
        /// <summary>
        /// Если не был выбран ни один из импортируемых столбцов
        /// </summary>
        #endregion
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