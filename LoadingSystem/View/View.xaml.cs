using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
			textBoxInfo.IsEnabled = true;
			buttonRead.IsEnabled = true;
			buttonTable.IsEnabled = true;
			toExcel.IsEnabled = true;

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

				var checkExport = new Util.CustomCheckBox
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					Index = i
				};
				checkExport.Click += CheckExport_Click;


				innerPanel.Children.Add(nameRow);
				innerPanel.Children.Add(typeRow);
				innerPanel.Children.Add(unitRow);
				innerPanel.Children.Add(checkExport);

				propertyPanel.Children.Add(innerPanel);
			}
		}



		private void CheckExport_Click(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as Util.CustomCheckBox;

			((ViewModel.ViewModel)this.DataContext).SetImportIndex(checkBox);
		}



		private void Grid_Drop(object sender, DragEventArgs e)
		{
			LoadDropedFile(e);
		}



		private void MainGrid_DragOver(object sender, DragEventArgs e)
		{
			dragDropImg.Visibility = Visibility.Visible;
		}



		private void MainGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			dragDropImg.Visibility = Visibility.Hidden;
		}



		protected void LoadDropedFile(DragEventArgs eventArgs)
		{
			if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = (string[])eventArgs.Data.GetData(DataFormats.FileDrop);

				((ViewModel.ViewModel)this.DataContext).ProcessIncomingFile(files[0]);
			}

			dragDropImg.Visibility = Visibility.Hidden;
		}



		private void TextBoxInfo_PreviewDragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.Text))
			{
				textBoxInfo.IsEnabled = false;
			}
		}
	}
}