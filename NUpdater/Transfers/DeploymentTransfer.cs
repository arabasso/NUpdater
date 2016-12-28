using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NUpdater.Transfers
{
    [XmlRoot("deployment")]
    [Serializable]
    public class DeploymentTransfer
    {
        [XmlElement("version")]
        public string Version { get; set; }
        [XmlElement("address")]
        public string Address { get; set; }
        [XmlArray("files")]
        [XmlArrayItem("file")]
        public List<DeploymentFileTransfer> Files { get; set; }
    }
}
