using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Jean_Doe.Common;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using System.Collections.Generic;
using System.Windows.Data;
using System.ComponentModel;
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for HistorySearch.xaml
    /// </summary>
    public partial class HistorySearch : IHandle<SearchResult>
    {
        int numOfHistory = 10;
        public HistorySearch()
        {
            InitializeComponent();
            MessageBus.Instance.Subscribe(this);
            Source = CollectionViewSource.GetDefaultView(HistoryItems);
            ItemsSource = Source;
            MinWidth = 200;
            MouseLeftButtonUp += HistorySearch_MouseLeftButtonUp;
        }
        ObservableCollection<HistorySearchItem> HistoryItems = new ObservableCollection<HistorySearchItem>();

        public void Load()
        {
            HistoryItems.Clear();
            try
            {
                //PersistHelper.Load<HistorySearchItem>().Take(numOfHistory).ToList().ForEach(x => HistoryItems.Add(x));
                HistorySearchItem.Defaults.ForEach(x => HistoryItems.Add(x));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        public void Save()
        {
            try
            {
                var items = HistoryItems.Where(x => HistorySearchItem.Defaults.IndexOf(x) == -1)
                    .ToArray();
                PersistHelper.Save(items);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        bool isChanging = false;
        public bool IsChanging { get { return isChanging; } }
        public void RemoveAt(int i)
        {
            HistoryItems.RemoveAt(i);
        }

        public void Insert(EnumSearchType t, string key, long counter)
        {
            isChanging = true;
            if (string.IsNullOrEmpty(key)) return;
            var k = key.ToLower().Trim();
            if (k == HistoryItems[0].Key && t == HistoryItems[0].SearchType)
            {
                isChanging = false;
                return;
            }
            if (HistoryItems.Count == numOfHistory + HistorySearchItem.Defaults.Count)
            {
                HistoryItems.RemoveAt(numOfHistory - 1);
            }
            HistoryItems.Insert(0, new HistorySearchItem
            {
                Key = k,
                SearchType = t,
                SearchCount = 1,
                ResultCount = counter,
            });
            SelectedIndex = 0;
            isChanging = false;
        }
        ICollectionView Source;
        void HistorySearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDropDownOpen && HistoryItems.Count > 0)
            {
                //IsDropDownOpen = true;
            }
        }
        public EnumSearchType SelectedSearchType
        {
            get
            {
                if (SelectedItem == null) return EnumSearchType.all;
                return (SelectedItem as HistorySearchItem).SearchType;
            }
        }
        public string SavePath { get; set; }

        public void Handle(SearchResult message)
        {
            if (message == null || string.IsNullOrEmpty(message.Keyword)) return;
            UIHelper.RunOnUI(() =>
            {
                this.Insert(message.SearchType, message.Keyword, message.Count);
                this.Save();
            });
        }
        public event TextChangedEventHandler TextChanged;
        private void ComboBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (TextChanged != null)
                TextChanged(sender, e);
        }
    }
    [XmlRoot("History")]
    public class XHistorySearch : List<HistorySearchItem>
    {
    }
}
