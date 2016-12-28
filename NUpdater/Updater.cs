using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUpdater
{
    public class Updater
    {
        private readonly Configuration _configuration;

        public Updater()
        {
            _configuration = Configuration.FromAppSettings();
        }

        public void Start()
        {
            var notify = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.Icon32x32
            };

            try
            {
                if (PriorProcess(Process.GetCurrentProcess()) != null) return;

                if (_configuration.ApplicationInstalled)
                {
                    RunApplication();
                }

                var deployment = Deployment.FromUri(_configuration);

                if (!deployment.ShouldUpdate()) return;

                notify.ShowBalloonTip(10000, "", string.Format(Properties.Resources.NewUpdate, _configuration.Path), ToolTipIcon.Info);

                foreach (var file in deployment.Files.Where(file => !file.HasTemp))
                {
                    file.Download();
                }

                if (_configuration.ApplicationInstalled)
                {
                    RunApplication();
                }

                else
                {
                    notify.ShowBalloonTip(10000, "", string.Format(Properties.Resources.SucessUpdate, _configuration.Path), ToolTipIcon.Info);
                }
            }

            catch (Exception ex)
            {
                notify.ShowBalloonTip(10000, "", ex.Message, ToolTipIcon.Error);
            }

            finally
            {
                notify.Visible = false;
            }
        }

        public void RunApplication()
        {
            if (_configuration.HasLocalDeploymentPath)
            {
                Deployment deployment;

                using (var stream = File.OpenRead(_configuration.LocalDeploymentPath))
                {
                    deployment = Deployment.FromStream(_configuration, stream);
                }

                if (deployment.UpdateIsPossible())
                {
                    deployment.Update();

                    DeleteTemp();
                }
            }

            if (_configuration.SingleInstance)
            {
                var p = PriorProcess(_configuration.ApplicationPath);

                if (p != null)
                {
                    AtivateInstance(p);

                    return;
                }
            }

            try
            {
                Process.Start(_configuration.ApplicationPath);
            }
            catch
            {
                // Ignore
            }
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

        public Process PriorProcess(Process curr)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var procs = Process.GetProcessesByName(curr.ProcessName);

            return procs.FirstOrDefault(p => (p.SessionId == currentSessionId) && (p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName));
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
    }
}
