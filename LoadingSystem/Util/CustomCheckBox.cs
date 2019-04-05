using System.ComponentModel;
using System.Windows.Controls;

namespace LoadingSystem.Util
{
	public class CustomCheckBox : CheckBox
	{
		private int index;

		public int Index
		{
			get { return index; }

			set
			{
				index = value;
				OnPropertyChanged("Index");
			}
		}


		public CustomCheckBox()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
