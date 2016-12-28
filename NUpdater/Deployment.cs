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
        public static Deployment FromStream(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(DeploymentTransfer));

            var transfer = (DeploymentTransfer)serializer.Deserialize(stream);

            var d = new Deployment
            {
                Address = transfer.Address,
                Version = new Version(transfer.Version)
            };

            d.Files = transfer.Files.Select(s => new DeploymentFile(d, s)).ToList();

            return d;
        }

        public static Deployment FromUri(Uri uri)
        {
            var request = WebRequest.Create(uri);

            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    return FromStream(stream);
                }
            }
        }

        public Version Version { get; set; }
        public string Address { get; set; }

        private List<DeploymentFile> _files;
        public List<DeploymentFile> Files
        {
            get { return _files ?? (_files = new List<DeploymentFile>()); }
            set { _files = value; }
        }
    }
}
