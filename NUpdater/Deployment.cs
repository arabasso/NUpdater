using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Xml.Serialization;
using NUpdater.Transfers;

namespace NUpdater
{
    public class Deployment
    {
        public static Deployment FromStream(Configuration configuration, Stream stream)
        {
            var serializer = new XmlSerializer(typeof(DeploymentTransfer));

            var transfer = (DeploymentTransfer)serializer.Deserialize(stream);

            var d = new Deployment
            {
                Configuration = configuration,
                Address = new Uri(transfer.Address),
                Version = new Version(transfer.Version)
            };

            d.Files = transfer.Files.Select(s => new DeploymentFile(d, s)).ToList();

            return d;
        }

        public static Deployment FromUri(Configuration configuration)
        {
            var request = configuration.CreateWebRequest(configuration.Address);

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    return FromStream(configuration, stream);
                }
            }
        }

        public Configuration Configuration { get; set; }
        public Version Version { get; set; }
        public Uri Address { get; set; }

        private List<DeploymentFile> _files;
        public List<DeploymentFile> Files
        {
            get { return _files ?? (_files = new List<DeploymentFile>()); }
            set { _files = value; }
        }

        public bool ShouldUpdate()
        {
            return Files.Any(deploymentFile => deploymentFile.ShouldUpdate());
        }

        public bool UpdateIsPossible()
        {
            return !Files.Any(a => a.IsLocked);
        }
    }
}
