using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoadingSystem.Model
{
    public class PropertiesModel
    {
        private string tbInfoStartsFrom;
        private string tbDataSetName;
        private string tbFieldName;
        private string tbBushName;
        private string tbWellName;
        private string tbDecimalSeparator;
        private string tbSeparator;

        #region Data Model

        public string TbInfoStartsFrom
        {
            get { return tbInfoStartsFrom; }

            set
            {
                tbInfoStartsFrom = value;
                OnPropertyChanged("TbInfoStartsFrom");
            }
        }

        public string TbDataSetName
        {
            get { return tbDataSetName; }

            set
            {
                tbDataSetName = value;
                OnPropertyChanged("TbDataSetName");
            }
        }

        public string TbFieldName
        {
            get { return tbFieldName; }

            set
            {
                tbFieldName = value;
                OnPropertyChanged("TbFieldName");
            }
        }

        public string TbBushName
        {
            get { return tbBushName; }

            set
            {
                tbBushName = value;
                OnPropertyChanged("TbBushName");
            }
        }

        public string TbWellName
        {
            get { return tbWellName; }

            set
            {
                tbWellName = value;
                OnPropertyChanged("TbWellName");
            }
        }

        public string TbDecimalSeparator
        {
            get { return tbDecimalSeparator; }

            set
            {
                tbDecimalSeparator = value;
                OnPropertyChanged("TbDecimalSeparator");
            }
        }

        public string TbSeparator
        {
            get { return tbSeparator; }

            set
            {
                tbSeparator = value;
                OnPropertyChanged("TbSeparator");
            }
        }

        #endregion

        public PropertiesModel()
        {

        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
