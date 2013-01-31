using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayer
{

    internal class CharmBarEntity
    {
        private Binding b = new Binding();

        public Binding Binding
        {
            get { return b; }
            set { b = value; }
        }
        public ObservableCollection<CharmAction> Actions = new ObservableCollection<CharmAction>();
    }
    internal class CharmAction : INotifyPropertyChanged
    {
        private string label;

        public string Label
        {
            get { return label; }
            set { label = value; Notify("Label"); }
        }

        public Action<object, RoutedEventArgs> Action { get; set; }
        public CharmAction(string l, Action<object, RoutedEventArgs> a)
        {
            Label = l;
            Action = a;
            Validate = (s,e) => { };
        }
        public CharmAction(string l, Action<object, RoutedEventArgs> a,Action<object,CharmAction> v)
        {
            Label = l;
            Action = a;
            Validate = v;
        }
        private bool isActive=true;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; Notify("IsActive"); }
        }

        public Action<object,CharmAction> Validate { get; set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
