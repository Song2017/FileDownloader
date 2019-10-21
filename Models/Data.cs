using System.Collections.Generic;
using System.Xml.Serialization;

namespace Models
{
    [XmlRoot(ElementName = "valve")]
    public class Data
    {
        [XmlElement(ElementName = "tag")]
        public string TagNumber { get; set; }
        [XmlElement(ElementName = "serialnumber")]
        public string SerialNumber { get; set; }
        [XmlElement(ElementName = "owner")]
        public string OwnerName { get; set; }
        [XmlElement(ElementName = "location")]
        public string PlantLocation { get; set; }
        [XmlElement(ElementName = "createdate")]
        public string CreateDate { get; set; }
        [XmlElement(ElementName = "repair")]
        public List<DataSub1> Repairs { get; set; }


        [XmlIgnore]
        public string Token { get; set; }
        [XmlIgnore]
        public string TenantKey { get; set; }
        [XmlIgnore]
        public string ValveTable { get; set; }
        [XmlIgnore]
        public string FileType { get; set; }
        [XmlIgnore]
        public string EquipmentKey { get; set; }
    }
}
