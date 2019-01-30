using System.ComponentModel;
using System.Data;

namespace LoadingSystem.Model
{
    public class DataGridModel
    {
        private DataTable dataGridTable = new DataTable();

        public DataTable DataGridTable
        {
            get { return dataGridTable; }

            set
            {
                dataGridTable = value;
                OnPropertyChanged("TestDataTable");
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
