using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LoadingSystem.View
{
	/// <summary>
	/// Interaction logic for View.xaml
	/// </summary>
	public partial class View : UserControl
	{

		public View()
		{
			InitializeComponent();

			DataContext = new ViewModel.ViewModel();
		}



		private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			e.Row.Header = (e.Row.GetIndex() + 1).ToString();
		}



		private void GridOfData_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			stackPanelScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
		}



		private void GridOfData_Loaded(object sender, RoutedEventArgs e)
		{
			var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
			if (dpd != null)
			{
				dpd.AddValueChanged(gridOfData, ItemsSourceIsChanged);
			}
		}



		private void ItemsSourceIsChanged(object sender, EventArgs e)
		{
			var depthValue = ((ViewModel.ViewModel)this.DataContext).DepthValue;

			propertyPanel.Children.Clear();
			buttonRead.IsEnabled = true;
			buttonTable.IsEnabled = true;
			toExcel.IsEnabled = true;
			toHTML.IsEnabled = true;

			for (int i = 0; i < gridOfData.Columns.Count; ++i)
			{
				var innerPanel = new StackPanel()
				{
					Orientation = Orientation.Vertical
				};

				var nameRow = new TextBox()
				{
					Width = gridOfData.ColumnWidth.Value,
					Text = $"H{i.ToString()}",
					TextAlignment = TextAlignment.Center,
					IsEnabled = false
				};

				var typeRow = new ComboBox()
				{
					Width = gridOfData.ColumnWidth.Value,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					ItemsSource = new List<string>()
					{
						"Depth", "RadioWaves", "LinearWaves"
					}
				};

				// Change, when "normal" itemsource will be added!
				if (i == depthValue)
				{
					typeRow.SelectedItem = "Depth";
				}

				var unitRow = new ComboBox()
				{
					Width = gridOfData.ColumnWidth.Value,
					HorizontalContentAlignment = HorizontalAlignment.Center,
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
		}



		private void Grid_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])e.Data.GetData(DataFormats.FileDrop);

				((ViewModel.ViewModel)this.DataContext).ProcessIncomingFile(files[0]);
			}

			dragDropImg.Visibility = Visibility.Hidden;
		}



		private void MainGrid_DragOver(object sender, DragEventArgs e)
		{
			dragDropImg.Visibility = Visibility.Visible;
		}



		private void MainGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			dragDropImg.Visibility = Visibility.Hidden;
		}
	}
}

#region Previous version
//private void OpenClick(object sender, RoutedEventArgs e)
//{
//	var openFileDialog = new OpenFileDialog();

//	if (openFileDialog.ShowDialog() == true)
//	{
//		var textTask = Task.Run(async () =>
//		{
//			return await Model.FileReader.ReadAllLinesAsync(openFileDialog.FileName);
//		});

//		var arrayOfData = textTask.Result;
//		var dataIndex = Model.BusinessLogic.GetDataIndex(arrayOfData);
//		var textBuilder = new StringBuilder();
//		var take = 2000;

//		for (int i = 0; i < dataIndex + take; ++i)
//		{
//			textBuilder.Append($"{arrayOfData[i]} \n");
//		}

//		textBoxInfo.Text = textBuilder.ToString();

//		var task = Task.Run(() =>
//		{
//			return Model.BusinessLogic.ReadData(arrayOfData, dataIndex, take);
//		});

//		var data = task.Result;

//		var columns = data[0].ListOfNumbers.Count;
//		var table = new Model.DataGridModel().DataGridTable;
//		table.Columns.Clear();
//		propertyPanel.Children.Clear();
//		table.BeginLoadData();

//		for (int i = 1; i <= columns; ++i)
//		{
//			table.Columns.Add(i.ToString());

//			var innerPanel = new StackPanel()
//			{
//				Orientation = Orientation.Vertical
//			};

//			var nameRow = new TextBox()
//			{
//				Width = gridOfData.ColumnWidth.Value,
//				Text = $"H{i.ToString()}",
//				IsEnabled = false
//			};

//			var typeRow = new ComboBox()
//			{
//				Width = gridOfData.ColumnWidth.Value,
//				ItemsSource = new List<string>()
//				{
//					"Depth", "RadioWaves", "LinearWaves"
//				}
//			};

//			var unitRow = new ComboBox()
//			{
//				Width = gridOfData.ColumnWidth.Value,
//				ItemsSource = new List<string>()
//				{
//					"Meter", "Kilometer", "Centimeter"
//				}
//			};

//			innerPanel.Children.Add(nameRow);
//			innerPanel.Children.Add(typeRow);
//			innerPanel.Children.Add(unitRow);

//			propertyPanel.Children.Add(innerPanel);
//		}

//		var content = new object[columns];

//		for (int i = 0; i < data.Count; ++i)
//		{
//			content = data[i].ListOfNumbers.Cast<object>().ToArray();
//			table.Rows.Add(content);
//		}

//		table.EndLoadData();

//		gridOfData.ItemsSource = table.DefaultView;

//	}
//}
#endregion