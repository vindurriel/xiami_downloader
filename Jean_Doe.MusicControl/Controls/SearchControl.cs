using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Artwork.MessageBus;
using Artwork.MessageBus.Interfaces;
using Jean_Doe.Common;

namespace Jean_Doe.MusicControl
{
    /// <summary>
    /// </summary>
    public class SearchControl : Control, IHandle<MsgSearchStateChanged>
    {
        static SearchControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchControl), new FrameworkPropertyMetadata(typeof(SearchControl)));
        }
        HistorySearch hs;
        Button btn_search;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            hs = GetTemplateChild("historySearch") as HistorySearch;
            refresh();
            btn_search = GetTemplateChild("btn_search") as Button;
            btn_search.DataContext = this;
            btn_search.Click += btn_search_Click;
        }
        void refresh()
        {
            if(hs == null) return;
            hs.SearchType = SearchType;
            hs.SavePath = SavePath;
            hs.Load();
        }


        public EnumXiamiType MusicType
        {
            get { return (EnumXiamiType)GetValue(MusicTypeProperty); }
            set { SetValue(MusicTypeProperty, value); }
        }

        public static readonly DependencyProperty MusicTypeProperty =
            DependencyProperty.Register("MusicType", typeof(EnumXiamiType), typeof(SearchControl), new UIPropertyMetadata(EnumXiamiType.song, musictype_changed));
        static void musictype_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == e.OldValue) return;
            var o = d as SearchControl;
            o.refresh();
        }


        public EnumSearchType SearchType
        {
            get { return (EnumSearchType)GetValue(SearchTypeProperty); }
            set
            {
                SetValue(SearchTypeProperty, value);
            }
        }
        public static readonly DependencyProperty SearchTypeProperty =
            DependencyProperty.Register("SearchType", typeof(EnumSearchType), typeof(SearchControl), new UIPropertyMetadata(EnumSearchType.key, searchtype_changed));
        static void searchtype_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == e.OldValue) return;
            var o = d as SearchControl;
            o.refresh();
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchControl), new UIPropertyMetadata("搜索"));

        public string SavePath { get { return System.IO.Path.Combine(Global.BasePath, "history_" + SearchType.ToString() + ".xml"); } }

        public SearchControl()
        {
            MessageBus.Instance.Subscribe(this);
        }


        async void btn_search_Click(object sender, RoutedEventArgs e)
        {
            switch(SearchManager.State)
            {
                case EnumSearchState.Started:
                    SearchManager.Cancel();
                    break;
                case EnumSearchState.Working:
                    SearchManager.Cancel();
                    break;
                case EnumSearchState.Cancelling:
                    break;
                case EnumSearchState.Finished:
                    await search();
                    break;
                default:
                    break;
            }
        }
        async Task search()
        {
            var key = hs.Text.Trim();
            EnumSearchType searchType = EnumSearchType.key;
            EnumXiamiType musicType = EnumXiamiType.any;
            Dispatcher.Invoke(new Action(() =>
            {
                searchType = SearchType;
                musicType = MusicType;
            }));
            switch(searchType)
            {
                case EnumSearchType.key:
                    await SearchManager.Search(key, musicType);
                    break;
                //case EnumSearchType.type:
                //    await SearchManager.SearchByType(key, musicType);
                //    break;
                case EnumSearchType.url:
                    await SearchManager.SearchByUrl(key);
                    break;
                default:
                    break;
            }
        }
        public void Handle(MsgSearchStateChanged message)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                switch(message.State)
                {
                    case EnumSearchState.Started:
                        SearchText = "停止";
                        break;
                    case EnumSearchState.Working:
                        SearchText = "停止";
                        break;
                    default:
                        SearchText = "搜索";
                        break;
                }
            }));
        }
    }
}
