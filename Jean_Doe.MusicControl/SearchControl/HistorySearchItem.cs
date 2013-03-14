using System.Xml;
using System.Xml.Serialization;
namespace Jean_Doe.MusicControl
{
    public class HistorySearchItem
    {
        [XmlElement("Key")]
        public XmlCDataSection XKey
        {
            get { return new XmlDocument().CreateCDataSection(Key); }
            set { Key = value.Value; }
        }
        [XmlIgnore]
        public string Key { get; set; }
        [XmlAttribute]
        public int SearchCount { get; set; }
        [XmlAttribute]
        public long ResultCount { get; set; }
    }
}
