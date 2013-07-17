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
        public HistorySearch()
        {
            InitializeComponent();
            MessageBus.Instance.Subscribe(this);
            Source = CollectionViewSource.GetDefaultView(HistoryItems);
            Source.Filter = (s) => { return HistoryItems.IndexOf(s as HistorySearchItem) < 10; };
            ItemsSource = Source;
            MinWidth = 200;
            MouseLeftButtonUp += HistorySearch_MouseLeftButtonUp;
        }
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
                using (var db = new SQLite.SQLiteConnection(PersistHelper.SqliteDbPath))
                {
                    db.CreateTable<HistorySearchItem>();
                    (from s in db.Table<HistorySearchItem>() select s).ToList().ForEach(x => HistoryItems.Add(x));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
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
                using (var db = new SQLite.SQLiteConnection(PersistHelper.SqliteDbPath))
                {
                    db.CreateTable<HistorySearchItem>();
                    db.BeginTransaction();
                    foreach (var item in HistoryItems)
                    {
                        db.InsertOrReplace(item);
                    }
                    db.Commit();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        public void Upsert(EnumSearchType t, string key, long counter)
        {
            if (string.IsNullOrEmpty(key)) return;
            var k = key.ToLower().Trim();
            var item = HistoryItems.FirstOrDefault(x => x.Key == k);
            if (item == null)
                HistoryItems.Insert(0, new HistorySearchItem
                {
                    Key = k,
                    SearchType = t,
                    SearchCount = 1,
                    ResultCount = counter,
                });
            else
            {
                item.SearchCount++;
                item.SearchType = t;
                item.ResultCount = counter;
                HistoryItems.Remove(item);
                HistoryItems.Insert(0, item);
            }
            SelectedIndex = 0;
            Save();
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
            UIHelper.RunOnUI(new Action(() =>
            {
                this.Upsert(message.SearchType, message.Keyword, message.Count);
                this.Save();
            }));
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
