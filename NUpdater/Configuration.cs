using System;
using System.Configuration;

namespace NUpdater
{
    public class Configuration
    {
        public Uri Address { get; set; } = new Uri("http://localhost/App/Deployment.xml");
        public string Executable { get; set; } = "App.exe";
        public string Path { get; set; } = "App";
        public bool ProxyEnable { get; set; }
        public string ProxyAddress { get; set; } = "http://192.168.1.1:3128";
        public bool AnonymousProxy { get; set; } = true;
        public string ProxyUser { get; set; } = "user";
        public string ProxyPassword { get; set; } = "pass";
        public string TempDir { get; set; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NUpdater");

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

            var tempDir = appSettings["TempDir"];

            cfg.TempDir = !string.IsNullOrEmpty(tempDir)
                ? Environment.ExpandEnvironmentVariables(tempDir)
                : System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NUpdater", cfg.Path);

            return cfg;
        }
    }
}
