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
            var listOfCurveNames = ((ViewModel.ViewModel)this.DataContext).DataModel.ListOfCurveNames;



			propertyPanel.Children.Clear();
			textBoxInfo.IsEnabled = true;
			buttonRead.IsEnabled = true;
			buttonTable.IsEnabled = true;
			toExcel.IsEnabled = true;
            tbCountOfRows.IsEnabled = true;
            tbImportFrom.IsEnabled = true;
            tbImportTo.IsEnabled = true;


            tbInfoStartsFrom.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbInfoStartsFrom;
            tbDataSetname.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbDataSetName;
            tbFieldName.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbFieldName;
            tbBushName.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbBushName;
            tbWellName.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbWellName;
            tbDecimalSeparator.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbDecimalSeparator;
            tbSeparator.Text = ((ViewModel.ViewModel)this.DataContext).PropertiesModel.TbSeparator;


            if (null != listOfCurveNames && listOfCurveNames.Count > 1)
            {
                PropertiesReadFromData(depthValue, listOfCurveNames);
            }
            else
            {
                StandartInitOfProperties(depthValue);
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



        protected void StandartInitOfProperties(int depthValue)
        {
            var brushConverter = new System.Windows.Media.BrushConverter();

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
                    Background = (System.Windows.Media.Brush)brushConverter.ConvertFrom("#ff177e89"),
                    Foreground = System.Windows.Media.Brushes.White,
                    IsEnabled = false
                };

                var typeRow = new ComboBox()
                {
                    Width = gridOfData.ColumnWidth.Value,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    ItemsSource = new List<string>()
                    {
                        "", "Depth"
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
                        "", "mRKB", "frac", "mD"
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



        protected void PropertiesReadFromData(int depthValue, List<string> listOfCurveNames)
        {
            var brushConverter = new System.Windows.Media.BrushConverter();

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
                    Background = (System.Windows.Media.Brush)brushConverter.ConvertFrom("#ff177e89"),
                    Foreground = System.Windows.Media.Brushes.White,
                    IsEnabled = false
                };

                var typeRow = new ComboBox()
                {
                    Name = "TypeRow",
                    Width = gridOfData.ColumnWidth.Value,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    ItemsSource = listOfCurveNames,
                    ContextMenu = GetContextMenuOfType()
                };


                // Change, when "normal" itemsource will be added!
                if (i == depthValue)
                {
                    typeRow.SelectedItem = "Depth";
                }
                else
                {
                    typeRow.SelectedItem = listOfCurveNames[i + 1];
                }

                var unitRow = new ComboBox()
                {
                    Width = gridOfData.ColumnWidth.Value,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    ItemsSource = new List<string>()
                    {
                        "", "mRKB", "frac", "mD"
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



        protected ContextMenu GetContextMenuOfType()
        {
            var contextMenu = new ContextMenu();

            var moveLeft = new MenuItem
            {
                Header = "Смещение влево"
            };
            moveLeft.Click += MoveLeft_Click;
            contextMenu.Items.Add(moveLeft);

            var moveRight = new MenuItem
            {
                Header = "Смещение вправо"
            };
            moveRight.Click += MoveRight_Click;
            contextMenu.Items.Add(moveRight);

            return contextMenu;
        }



        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            var items = ((ViewModel.ViewModel)this.DataContext).DataModel.ListOfCurveNames; ;
            var length = items.Count;
            var firstElement = items[0];
            
            for (int i = 0; i < length - 1; ++i)
            {
                items[i] = items[i + 1];
            }

            items[length - 1] = firstElement;

            var childIndex = 0;
            foreach (var item in propertyPanel.Children)
            {
                var element = item as StackPanel;

                foreach (var child in element.Children)
                {
                    if (child is ComboBox innerElement)
                    {
                        if (innerElement.Name == "TypeRow")
                        {
                            innerElement.SelectedItem = items[childIndex];
                            childIndex++;
                        }
                    }
                }
            }
        }



        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            var items = ((ViewModel.ViewModel)this.DataContext).DataModel.ListOfCurveNames; ;
            var length = items.Count;
            var lastElement = items[length - 1];

            for (int i = length - 1; i > 0; --i)
            {
                items[i] = items[i - 1];
            }

            items[0] = lastElement;

            var childIndex = 0;
            foreach (var item in propertyPanel.Children)
            {
                var element = item as StackPanel;

                foreach (var child in element.Children)
                {
                    if (child is ComboBox innerElement)
                    {
                        if (innerElement.Name == "TypeRow")
                        {
                            innerElement.SelectedItem = items[childIndex];
                            childIndex++;
                        }
                    }   
                }
            }
        }



        private void TbCountOfRows_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RebootText();
            }
        }



        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            RebootText();
        }



        private void TbImportFrom_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RebootTable();
            }
        }



        private void TbImportTo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RebootTable();
            }
        }



        private void ButtonTable_Click(object sender, RoutedEventArgs e)
        {
            RebootTable();
        }



        protected void RebootTable()
        {
            if (int.TryParse(tbImportFrom.Text, out var importFrom) && int.TryParse(tbImportTo.Text, out var importTo))
            {
                ((ViewModel.ViewModel)this.DataContext).ChangeTable(importFrom, importTo);
            }
            else
            {
                tbImportTo.Text = string.Empty;
                MessageBox.Show("Вводить можно только числа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        protected void RebootText()
        {
            if (int.TryParse(tbCountOfRows.Text, out var readTo))
            {
                ((ViewModel.ViewModel)this.DataContext).ChangeTextBoxAsync(readTo);
            }
            else
            {
                tbCountOfRows.Text = string.Empty;
                MessageBox.Show("Вводить можно только числа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}