using System;
using System.Configuration;
using System.Net;
using System.Net.Cache;

namespace NUpdater
{
    public class Configuration
    {
        private string _tempDir;
        public Uri Address { get; set; } = new Uri("http://localhost/App/Deployment.xml");
        public string Executable { get; set; } = "App.exe";
        public string Path { get; set; } = "App";
        public bool ProxyEnable { get; set; }
        public string ProxyAddress { get; set; } = "http://192.168.1.1:3128";
        public bool AnonymousProxy { get; set; } = true;
        public string ProxyUser { get; set; } = "user";
        public string ProxyPassword { get; set; } = "pass";

        public string TempDir
        {
            get
            {
                if (string.IsNullOrEmpty(_tempDir))
                {
                    _tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NUpdater", Path);
                }

                return _tempDir;
            }
            set { _tempDir = value; }
        }

        public static Configuration FromAppSettings()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var cfg = new Configuration
            {
                Address = new Uri(appSettings["Address"]),
                Path = appSettings["Path"],
                AnonymousProxy = bool.Parse(appSettings["AnonymousProxy"]),
                Executable = appSettings["Executable"],
                ProxyAddress = appSettings["ProxyAddress"],
                ProxyEnable = bool.Parse(appSettings["ProxyEnable"]),
                ProxyPassword = appSettings["ProxyPassword"],
                ProxyUser = appSettings["ProxyUser"]
            };

            return cfg;
        }

        public WebRequest CreateWebRequest(Uri address)
        {
            var request = WebRequest.Create(address);

            if (ProxyEnable)
            {
                request.Proxy = new WebProxy
                {
                    Address = new Uri(ProxyAddress),
                };

                if (!AnonymousProxy)
                {
                    request.Proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
                }
            }

            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            return request;
        }
    }
}
