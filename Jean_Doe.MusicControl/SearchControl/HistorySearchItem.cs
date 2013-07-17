using Jean_Doe.Common;
using System.Xml;
using System.Xml.Serialization;
using SQLite;
namespace Jean_Doe.MusicControl
{
    public class HistorySearchItem
    {
        [XmlElement("Key")][Ignore]
        public XmlCDataSection XKey
        {
            get { return new XmlDocument().CreateCDataSection(Key); }
            set { Key = value.Value; }
        }
        [XmlIgnore][PrimaryKey]
        public string Key { get; set; }
        [XmlAttribute]
        public EnumSearchType SearchType { get; set; }
        [XmlAttribute]
        public int SearchCount { get; set; }
        [XmlAttribute]
        public long ResultCount { get; set; }
    }
}
