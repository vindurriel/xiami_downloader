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
namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// Interaction logic for HistorySearch.xaml
    /// </summary>
    public partial class HistorySearch : ComboBox, IHandle<SearchResult>
    {
        public EnumSearchType SearchType { get; set; }
        ObservableCollection<HistorySearchItem> HistoryItems = new ObservableCollection<HistorySearchItem>();
        public void Load()
        {
            if (SavePath == null || !System.IO.File.Exists(SavePath))
            {
                return;
            }
            HistoryItems.Clear();
            try
            {
                PersistHelper.Load<XHistorySearch>(SavePath).Items.ForEach(x=> HistoryItems.Add(x));
            }
            catch (Exception){}
        }
        public void Save()
        {
            if (SavePath == null)
            {
                MessageBox.Show("save path not defined.");
                return;
            }
            try
            {
                var dir = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                PersistHelper.Save(new XHistorySearch { Items = HistoryItems.ToList() }, SavePath);
            }
            catch { }
        }
        public void Upsert(string key, long counter)
        {
            if (string.IsNullOrEmpty(key)) return;
            var k = key.ToLower().Trim();
            var item = HistoryItems.FirstOrDefault(x => x.Key == k);
            if (item == null)
                HistoryItems.Insert(0, new HistorySearchItem
                {
                    Key = k,
                    SearchCount = 1,
                    ResultCount = counter,
                });
            else
            {
                item.SearchCount++;
                item.ResultCount = counter;
                HistoryItems.Remove(item);
                HistoryItems.Insert(0, item);
            }
            SelectedIndex = 0;
        }
        public HistorySearch()
        {
            InitializeComponent();
            MessageBus.Instance.Subscribe(this);
            IsEditable = true;
            ItemsSource = HistoryItems;
            MinWidth = 200;
            MouseLeftButtonUp += HistorySearch_MouseLeftButtonUp;
        }

        void HistorySearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDropDownOpen && HistoryItems.Count > 0)
            {
                //IsDropDownOpen = true;
            }
        }
        public string SavePath { get; set; }

        public void Handle(SearchResult message)
        {
            if (message == null || message.SearchType != SearchType) return;
            UIHelper.RunOnUI(new Action(() =>
            {
                this.Upsert(message.Keyword, message.Count);
                this.Save();
            }));
        }
    }
    [XmlRoot("History")]
    public class XHistorySearch
    {
        [XmlArray("Searches")]
        [XmlArrayItem("Search")]
        public List<HistorySearchItem> Items { get; set; }
    }
}
