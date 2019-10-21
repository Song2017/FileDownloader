using System.Collections.Generic;
using System.Xml.Serialization;

namespace Models
{
    [XmlRoot(ElementName = "repair")]
    public class DataSub1
    {
        [XmlElement(ElementName = "effectivedate")]
        public string EffectiveDate { get; set; }
        [XmlElement(ElementName = "maintfor")]
        public string MaintenanceFor { get; set; }
        [XmlElement(ElementName = "part")]
        public List<DataSub2> Parts { get; set; }
    }
}
