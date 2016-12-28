using System;
using System.Xml.Serialization;

namespace NUpdater.Transfers
{
    [XmlRoot("file")]
    [Serializable]
    public class DeploymentFileTransfer
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("size")]
        public long Size { get; set; }
        [XmlAttribute("date")]
        public DateTime Date { get; set; }
        [XmlAttribute("hash")]
        public string Hash { get; set; }
    }
}
