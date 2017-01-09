using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var d = new Deployment(configuration)
            {
                Address = new Uri(transfer.Address),
                Version = new Version(transfer.Version)
            };

            d.Files = transfer.Files.Select(s => new DeploymentFile(d, s)).ToList();

            return d;
        }

        private static readonly List<string>
            ExcludedList = new List<string>
            {
                "*.pdb", "*.vshost.exe", "*.vshost.exe.config", "*.vshost.exe.manifest",
                "*.pssym"
            };

        public static Deployment FromAssembly(Configuration configuration, string source, List<string> excludedList = null)
        {
            if (excludedList != null)
            {
                ExcludedList.AddRange(excludedList);
            }

            var path = Path.Combine(source, configuration.Executable);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);

            var d = new Deployment(configuration)
            {
                Version = new Version(fileVersionInfo.ProductVersion)
            };

            var baseUri = new Uri(configuration.Address.ToString().Replace("Deployment.xml", string.Empty));

            d.Address = new Uri(baseUri, d.BuildVersion + "/");

            var files = Directory
                .EnumerateFiles(source, "*.*", SearchOption.AllDirectories)
                .Where(w => !IsExcluded(source, w, ExcludedList));

            foreach (var file in files)
            {
                d.Files.Add(new DeploymentFile(d, source, file));
            }

            return d;
        }

        private static bool IsExcluded(string source, string file, List<string> excludedList)
        {
            return excludedList
                .Select(s => Path.Combine(source, s))
                .Any(file.Like);
        }

        public static Deployment FromTemp(Configuration configuration)
        {
            using (var stream = File.OpenRead(configuration.TempDeploymentPath))
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

        public Deployment(Configuration configuration)
        {
            Configuration = configuration;
        }

        public List<DeploymentFile> Files
        {
            get { return _files ?? (_files = new List<DeploymentFile>()); }
            set { _files = value; }
        }

        public bool ShouldUpdate()
        {
            return Files.Any(a => a.ShouldUpdate());
        }

        public bool IsValid()
        {
            return Files.All(a => a.IsValid() || a.HasTemp);
        }

        public bool HasTemp()
        {
            return Files.All(a => a.HasTemp);
        }

        public bool ShouldDownload()
        {
            return Files.Any(a => a.ShouldDownload());
        }

        public long TotalDownload
        {
            get { return Files.Where(w => w.ShouldDownload()).Sum(s => s.Size); }
        }

        public string BuildVersion => Version.ToString().Replace('.', '_');

        public bool UpdateIsPossible()
        {
            return !Files.Any(a => a.IsLocked) && ShouldUpdate();
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

        public void Save(string file)
        {
            var directory = Path.GetDirectoryName(file);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = File.Open(file, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(DeploymentTransfer));

                var transfer = ToTransfer();

                serializer.Serialize(stream, transfer);
            }

            foreach (var deploymentFile in Files)
            {
                Console.WriteLine(@"Deploying {0}", deploymentFile.Name);

                deploymentFile.Save(directory);
            }
        }

        public void SaveTemp()
        {
            Save(Configuration.TempDeploymentPath);
        }
    }
}
