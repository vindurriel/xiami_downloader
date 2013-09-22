using Jean_Doe.Common;
using System.Xml;
using System.Xml.Serialization;
using SQLite;
using System.Collections.Generic;
namespace Jean_Doe.MusicControl
{
    public class HistorySearchItem
    {
        [XmlElement("Key")]
        [Ignore]
        public XmlCDataSection XKey
        {
            get { return new XmlDocument().CreateCDataSection(Key); }
            set { Key = value.Value; }
        }
        [XmlIgnore]
        [PrimaryKey]
        public string Key { get; set; }
        [XmlAttribute]
        public EnumSearchType SearchType { get; set; }
        [XmlAttribute]
        public int SearchCount { get; set; }
        [XmlAttribute]
        public long ResultCount { get; set; }
        [Ignore]
        public string FriendlyName
        {
            get
            {
                if (Key.StartsWith("user"))
                {
                    if (Key == "user:collect_recommend")
                        return "【推荐的精选集】";
                    if (Key == "user:daily")
                        return "【推荐的每日歌曲】";
                    if (Key == "user:guess")
                        return "【虾米猜】";
                    return string.Format("【收藏的{0}】", SearchType.GetDescription());
                }
                return string.Format("{0} {1}", SearchType.GetDescription(), Key);
            }
        }
        public static List<HistorySearchItem> Defaults = new List<HistorySearchItem> { 
            new HistorySearchItem{ Key="user:song",SearchType=EnumSearchType.song},
            new HistorySearchItem{ Key="user:artist",SearchType=EnumSearchType.artist},
            new HistorySearchItem{ Key="user:collect",SearchType=EnumSearchType.collect},
            new HistorySearchItem{ Key="user:album",SearchType=EnumSearchType.album},
            new HistorySearchItem{ Key="user:collect_recommend",SearchType=EnumSearchType.collect},
            new HistorySearchItem{ Key="user:guess",SearchType=EnumSearchType.song},
            new HistorySearchItem{ Key="user:daily",SearchType=EnumSearchType.song},
        };
    }
}
