using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUpdater.Transfers;

namespace NUpdater
{
    public class DownloadEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
    }

    public delegate void DownloadEventHandler(DownloadEventArgs e);

    public class UpdateEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
    }

    public delegate void UpdateEventHandler(UpdateEventArgs e);

    public class DownloadProgressEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public float Percent => (float)100.0 * Index / File.Size;
    }

    public delegate void DownloadProgressEventHandler(DownloadProgressEventArgs e);

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

        public static Deployment FromCache(Configuration configuration)
        {
            using (var stream = File.OpenRead(configuration.LocalDeploymentPath))
            {
                return FromStream(configuration, stream);
            }
        }

        public static Deployment FromUri(Configuration configuration)
        {
            var request = configuration.CreateWebRequest(configuration.Address);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                return FromStream(configuration, stream);
            }
        }

        public event DownloadProgressEventHandler DownloadProgress;

        public void OnDownloadProgress(DeploymentFile file, int count, int index)
        {
            DownloadProgress?.Invoke(new DownloadProgressEventArgs
            {
                File = file,
                Count = count,
                Index = index
            });
        }

        public event DownloadEventHandler StartDownload;

        public void OnStartDownload(DeploymentFile file)
        {
            StartDownload?.Invoke(new DownloadEventArgs
            {
                File = file,
            });
        }

        public event UpdateEventHandler UpdateFile;

        public void OnUpdateFile(DeploymentFile file)
        {
            UpdateFile?.Invoke(new UpdateEventArgs
            {
                File = file,
            });
        }

        public event DownloadEventHandler DownloadCompleted;

        public void OnDownloadCompleted(DeploymentFile file)
        {
            DownloadCompleted?.Invoke(new DownloadEventArgs
            {
                File = file,
            });
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
            return Files.Any(a => a.ShouldUpdate());
        }

        public bool ShouldDownload()
        {
            return Files.Any(a => a.ShouldDownload());
        }

        public long TotalDownload
        {
            get { return Files.Where(w => w.ShouldDownload()).Sum(s => s.Size); }
        }

        public bool UpdateIsPossible()
        {
            return !Files.Any(a => a.IsLocked) && !Files.Any(a => a.ShouldDownload());
        }

        public void Update()
        {
            foreach (var file in Files.Where(w => !w.IsLocked))
            {
                file.Update();
            }
        }

        public DeploymentTransfer ToTransfer()
        {
            return new DeploymentTransfer
            {
                Address = Address.ToString(),
                Version = Version.ToString(),
                Files = Files.Select(s => s.ToTransfer()).ToList()
            };
        }

        public void SaveLocal(string file)
        {
            using (var stream = File.OpenWrite(file))
            {
                var serializer = new XmlSerializer(typeof(DeploymentTransfer));

                var transfer = ToTransfer();

                serializer.Serialize(stream, transfer);
            }
        }

        public void SaveLocal()
        {
            if (!Configuration.HasTempDir)
            {
                Directory.CreateDirectory(Configuration.TempDir);
            }

            SaveLocal(Configuration.LocalDeploymentPath);
        }
    }
}
