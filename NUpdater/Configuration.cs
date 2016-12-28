using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.InteropServices;

namespace NUpdater
{
    public enum ShowWindowCommands
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window
        /// for the first time.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        ShowMinimized = 2,
        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        Maximize = 3, // is this the right value?
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>      
        ShowMaximized = 3,
        /// <summary>
        /// Displays a window in its most recent size and position. This value
        /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
        /// the window is not activated.
        /// </summary>
        ShowNoActivate = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level
        /// window in the Z order.
        /// </summary>
        Minimize = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to
        /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
        /// window is not activated.
        /// </summary>
        ShowMinNoActive = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is
        /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
        /// window is not activated.
        /// </summary>
        ShowNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        Restore = 9,
        /// <summary>
        /// Sets the show state based on the SW_* value specified in the
        /// STARTUPINFO structure passed to the CreateProcess function by the
        /// program that started the application.
        /// </summary>
        ShowDefault = 10,
        /// <summary>
        ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
        /// that owns the window is not responding. This flag should only be
        /// used when minimizing windows from a different thread.
        /// </summary>
        ForceMinimize = 11
    }

    public class Configuration
    {
        private string _tempDir;
        public Uri Address { get; set; } = new Uri("http://localhost/App/Deployment.xml");
        public bool SingleInstance { get; set; } = true;
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

        public bool ApplicationInstalled => Directory.Exists(Path) && File.Exists(ApplicationPath);

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

            if (!string.IsNullOrEmpty(appSettings["TempDir"]))
            {
                cfg.TempDir = Environment.ExpandEnvironmentVariables(appSettings["TempDir"]);
            }

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

        public string ApplicationPath => System.IO.Path.Combine(Path, Executable);
        public Icon ApplicationIcon => Icon.ExtractAssociatedIcon(ApplicationPath);

        public void RunApplication()
        {
            if (SingleInstance)
            {
                var p = ProgramIsRunning();

                if (p != null)
                {
                    AtivateInstance(p);

                    return;
                }
            }

            try
            {
                Process.Start(ApplicationPath);
            }
            catch
            {
                // Ignore
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        private const int KeyeventfExtendedkey = 0x1;
        private const int KeyeventfKeyup = 0x2;
        private const int VkMenu = 0x12;
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void SetForegroundWindowInternal(IntPtr hWnd)
        {
            var keyState = new byte[256];

            if (GetKeyboardState(keyState))
            {
                if ((keyState[VkMenu] & 0x80) == 0)
                {
                    keybd_event(VkMenu, 0, KeyeventfExtendedkey | 0, 0);
                }
            }

            SetForegroundWindow(hWnd);

            if (!GetKeyboardState(keyState)) return;

            if ((keyState[VkMenu] & 0x80) == 0)
            {
                keybd_event(VkMenu, 0, KeyeventfExtendedkey | KeyeventfKeyup, 0);
            }
        }

        public void AtivateInstance(Process processo)
        {
            if (processo == null) return;

            SetForegroundWindowInternal(processo.MainWindowHandle);
            ShowWindow(processo.MainWindowHandle, ShowWindowCommands.Restore);
        }

        public Process ProgramIsRunning()
        {
            var fileNameToFilter = System.IO.Path.GetFullPath(ApplicationPath);
            
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    var fileName = System.IO.Path.GetFullPath(p.MainModule.FileName);

                    if (string.Compare(fileNameToFilter, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return p;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            return null;
        }

        public void DeleteTemp()
        {
            foreach (var file in Directory.EnumerateFiles(TempDir))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}
