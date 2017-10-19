using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ASSRT
{
    public class DataClass : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private SolidColorBrush _Color;

        public SolidColorBrush Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                if (value != this._Color)
                {
                    this._Color = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DataClass(String Name)
        {
            this.Name = Name;
            this.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00aeef"));
        }

        public void ChangeColor()
        {
            this.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00bb30"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(""));
            }
        }
    }
}
