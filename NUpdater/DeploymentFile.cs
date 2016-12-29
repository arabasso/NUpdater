using System;
using System.IO;
using System.IO.Compression;
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

        public bool ShouldDownload()
        {
            return ShouldUpdate() && !HasTemp;
        }

        public bool ShouldUpdate()
        {
            var localPath = Path.Combine(Deployment.Configuration.Path, Name);

            if (!File.Exists(localPath)) return true;

            using (var stream = File.OpenRead(localPath))
            {
                var hash = stream.ToMd5();

                return hash != Hash;
            }
        }

        public string LocalPath => Path.Combine(Deployment.Configuration.Path, Name);
        public string TempPath => Path.Combine(Deployment.Configuration.TempDir, Name);
        public bool HasTemp
        {
            get
            {
                if (!File.Exists(TempPath)) return false;

                using (var stream = File.OpenRead(TempPath))
                {
                    return stream.ToMd5() == Hash;
                }
            }
        }

        public bool HasLocal => File.Exists(LocalPath);

        public bool IsLocked
        {
            get
            {
                if (!File.Exists(LocalPath)) return false;

                FileStream stream = null;

                try
                {
                    stream = File.Open(LocalPath, FileMode.Open, FileAccess.Read, FileShare.None);
                }

                catch (IOException)
                {
                    return true;
                }

                finally
                {
                    stream?.Close();
                }

                return false;
            }
        }

        public void Download()
        {
            if (!Deployment.Configuration.HasTempDir)
            {
                Directory.CreateDirectory(Deployment.Configuration.TempDir);
            }

            var uri = new Uri(Deployment.Address, Name + ".deploy");

            var request = Deployment.Configuration.CreateWebRequest(uri);

            var path = Path.GetDirectoryName(TempPath);

            if (path != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Deployment.OnStartDownload(this);

            var index = 0;

            using (var response = request.GetResponse())
            using (var streamIn = response.GetResponseStream())
            using (var stream = new DeflateStream(streamIn, CompressionMode.Decompress))
            using (var streamOut = File.OpenWrite(TempPath))
            {
                int count;
                var buffer = new byte[0x4000];

                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    index += count;

                    Deployment.OnDownloadProgress(this, count, index);

                    streamOut.Write(buffer, 0, count);
                }
            }

            Deployment.OnDownloadCompleted(this);

            if (!HasTemp)
            {
                throw new BadImageFormatException();
            }
        }

        public void DeleteTemp()
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }
        }

        public void Update()
        {
            if (!ShouldUpdate()) return;

            if (!HasTemp)
            {
                Download();
            }

            var localDir = Path.GetDirectoryName(LocalPath);

            if (localDir != null && !Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
            }

            Deployment.OnUpdateFile(this);

            File.Copy(TempPath, LocalPath, true);
        }

        public DeploymentFileTransfer ToTransfer()
        {
            return new DeploymentFileTransfer
            {
                Name = Name,
                Hash = Hash,
                Date = Date,
                Size = Size
            };
        }
    }
}
