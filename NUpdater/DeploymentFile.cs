using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using NUpdater.Transfers;

namespace NUpdater
{
    public class DeploymentFile
    {
        public Deployment Deployment { get; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime Date { get; set; }
        public string Hash { get; set; }

        public DeploymentFile(Deployment deployment, DeploymentFileTransfer transfer)
        {
            Deployment = deployment;
            Name = transfer.Name;
            Size = transfer.Size;
            Date = transfer.Date;
            Hash = transfer.Hash;
        }

        public bool ShouldUpdate()
        {
            if (!WasDownloaded) return false;

            using (var stream = File.OpenRead(TempPath))
            {
                var hash = stream.ToMd5();

                return hash != Hash;
            }
        }

        public string TempPath => Path.Combine(Deployment.Configuration.TempDir, Name);
        public bool WasDownloaded => File.Exists(TempPath);

        public void Download()
        {
            var tempDir = Deployment.Configuration.TempDir;

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var uri = new Uri(Deployment.Address, Name + ".deploy");

            var request = WebRequest.Create(uri);

            using (var response = request.GetResponse())
            using (var streamIn = response.GetResponseStream())
            using (var stream = new DeflateStream(streamIn, CompressionMode.Decompress))
            using (var streamOut = File.OpenWrite(TempPath))
            {
                int count;
                var buffer = new byte[0x4000];

                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    streamOut.Write(buffer, 0, count);
                }
            }
        }

        public void DeleteTempPath()
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }
        }
    }
}
