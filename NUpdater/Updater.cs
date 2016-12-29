using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUpdater
{
    public class Updater : IDisposable
    {
        private readonly Configuration _configuration;
        private Deployment _deployment;

        public Deployment Deployment => _deployment ?? (_deployment = Deployment.FromUri(_configuration));

        private long? _totalDownload;
        private readonly NotifyIcon _notify;
        public long TotalDownload => _totalDownload ?? (long)(_totalDownload = Deployment.TotalDownload);

        public Updater()
        {
            _notify = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.Icon_ico
            };

            _configuration = Configuration.FromAppSettings();
        }

        public event DownloadProgressEventHandler DownloadProgress;

        protected void OnDownloadProgress(DownloadProgressEventArgs args)
        {
            DownloadProgress?.Invoke(args);
        }

        public event DownloadEventHandler StartDownload;

        protected void OnStartDownload(DownloadEventArgs args)
        {
            StartDownload?.Invoke(args);
        }

        public event DownloadEventHandler DownloadCompleted;

        protected void OnDownloadCompleted(DownloadEventArgs args)
        {
            DownloadCompleted?.Invoke(args);
        }

        public event UpdateEventHandler UpdateFile;

        protected void OnUpdateFile(UpdateEventArgs args)
        {
            UpdateFile?.Invoke(args);
        }

        public void Start()
        {
            try
            {
                Deployment.SaveLocal();

                var message = string.Format(Properties.Resources.NewUpdate, _configuration.Name, Deployment.Version);

                _notify.ShowBalloonTip(10000, Deployment.Configuration.Company, message, ToolTipIcon.Info);

                Deployment.DownloadProgress += OnDownloadProgress;
                Deployment.StartDownload += OnStartDownload;
                Deployment.DownloadCompleted += OnDownloadCompleted;

                foreach (var file in Deployment.Files.Where(file => file.ShouldDownload()))
                {
                    file.Download();
                }
            }

            catch (Exception ex)
            {
                Log(ex);
            }

            finally
            {
                _notify.Visible = false;
            }
        }

        public void RunApplication()
        {
            if (_configuration.HasLocalDeploymentPath)
            {
                var deployment = Deployment.FromCache(_configuration);

                if (deployment.UpdateIsPossible())
                {
                    deployment.UpdateFile += OnUpdateFile;

                    deployment.Update();

                    DeleteTemp();
                }
            }

            if (_configuration.SingleInstance)
            {
                var p = PriorApplicationProcess();

                if (p != null)
                {
                    AtivateInstance(p);

                    return;
                }
            }

            if (!_configuration.ApplicationInstalled) return;

            try
            {
                Process.Start(_configuration.ApplicationPath);
            }

            catch
            {
                // Ignore
            }
        }

        public Process PriorApplicationProcess()
        {
            var p = PriorProcess(_configuration.ApplicationPath);
            return p;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int KeyeventfExtendedkey = 0x1;
        private const int KeyeventfKeyup = 0x2;
        private const int VkMenu = 0x12;
        private const int Restore = 9;
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
            ShowWindow(processo.MainWindowHandle, Restore);
        }

        public Process PriorProcess(string path)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var fileNameToFilter = Path.GetFullPath(path);

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    var fileName = Path.GetFullPath(p.MainModule.FileName);

                    if (currentSessionId != p.SessionId) continue;

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
            foreach (var file in Directory.EnumerateFiles(_configuration.TempDir, "*.*", SearchOption.AllDirectories))
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

            foreach (var dir in Directory.EnumerateDirectories(_configuration.TempDir))
            {
                try
                {
                    Directory.Delete(dir);
                }

                catch
                {
                    // Ignore
                }
            }
        }

        public bool ShouldUpdate()
        {
            return Deployment.ShouldUpdate();
        }

        public bool ShouldDownload()
        {
            return Deployment.ShouldDownload();
        }

        public void Log(Exception exception)
        {
            _notify.ShowBalloonTip(10000, "", exception.Message, ToolTipIcon.Error);
        }

        public void Dispose()
        {
            _notify.Visible = false;
        }
    }
}
