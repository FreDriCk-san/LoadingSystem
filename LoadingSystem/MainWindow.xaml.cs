using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LoadingSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                var task = Task.Run(async () =>
                {
                    return await Model.BusinessLogic.ReadFromFileAsync(openFileDialog.FileName);
                });

                var text = task.Result;
                var builder = new StringBuilder();

                for (int i = 1; i < text.Length; ++i)
                {
                    builder.Append(i + ":\n");
                }

                textBoxNum.Text = builder.ToString();
                textBoxInfo.Text = text;


                var textStartsFrom = Model.BusinessLogic.InfoStartsFrom(text, 6);

                var columns = Model.BusinessLogic.CountOfColumns(textStartsFrom);
                var listOfNumbers = Model.BusinessLogic.GetValues(textStartsFrom, '.', ',');


                var table = new Model.DataGridModel().DataGridTable;
                table.Columns.Clear();
                propertyPanel.Children.Clear();
                table.BeginLoadData();

                for (int i = 1; i <= 20; ++i)
                {
                    table.Columns.Add(i.ToString());

                    var innerPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };

                    var nameRow = new TextBox()
                    {
                        Width = gridOfData.ColumnWidth.Value,
                        Text = $"H{i.ToString()}",
                        IsEnabled = false
                    };

                    var typeRow = new ComboBox()
                    {
                        Width = gridOfData.ColumnWidth.Value,
                        ItemsSource = new List<string>()
                        {
                            "Depth", "RadioWaves", "LinearWaves"
                        }
                    };

                    var unitRow = new ComboBox()
                    {
                        Width = gridOfData.ColumnWidth.Value,
                        ItemsSource = new List<string>()
                        {
                            "Meter", "Kilometer", "Centimeter"
                        }
                    };

                    innerPanel.Children.Add(nameRow);
                    innerPanel.Children.Add(typeRow);
                    innerPanel.Children.Add(unitRow);

                    propertyPanel.Children.Add(innerPanel);
                }

                var content = new object[columns];
                var arrayOfNumbers = new string[listOfNumbers.Count];
                var decimalCounter = Model.BusinessLogic.GetDecimalNumberCount(textStartsFrom, '.', ',');


                for (int i = 0; i < arrayOfNumbers.Count(); ++i)
                {
                    arrayOfNumbers[i] = string.Format("{0:f" + decimalCounter + "}", listOfNumbers[i]);
                }

                for (int i = 0, j = 0; j < listOfNumbers.Count / columns; ++j , i += columns)
                {
                    content = arrayOfNumbers.Skip(i).Take(columns).Cast<object>().ToArray();
                    table.Rows.Add(content);
                }

                table.EndLoadData();

                gridOfData.ItemsSource = table.DefaultView;

            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void MainInfo_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var textToSync = (sender == textBoxNum) ? textBoxInfo : textBoxNum;

            textToSync.ScrollToVerticalOffset(e.VerticalOffset);
            textToSync.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void GridOfData_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            stackPanelScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        }
    }

}
