using System.Xml.Serialization;

namespace Models
{
    [XmlRoot(ElementName = "part")]
    public class DataSub2
    {
        [XmlElement(ElementName = "quantity")]
        public string PartQuantity { get; set; }
        [XmlElement(ElementName = "number")]
        public string PartNumber { get; set; }
        [XmlElement(ElementName = "name")]
        public string PartName { get; set; }
        [XmlElement(ElementName = "workperformed")]
        public string WorkPerformed { get; set; }
    }
}
