using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NUpdater
{
    public class Updater
    {
        protected readonly Configuration Configuration;
        private Deployment _deployment;
        private UpdateForm _updateForm;
        private BindingSource _registryConfigurationBindingSource;
        private ToolStripMenuItem _menuItemRestore;

        public Deployment Deployment => _deployment ?? (_deployment = Deployment.FromUri(Configuration));

        private long? _totalDownload;
        public long TotalDownload => _totalDownload ?? (long)(_totalDownload = Deployment.TotalDownload);

        public static Updater Create()
        {
            var configuration = Configuration.FromAppSettings();

            switch (configuration.Execution)
            {
                case Execution.Before: return new UpdaterExecutionBefore(configuration);
                case Execution.After: return new UpdaterExecutionAfter(configuration);
                default: return new Updater(configuration);
            }
        }

        protected Updater(
            Configuration configuration)
        {
            Configuration = configuration;
        }

        public void Run()
        {
            var registryConfiguration = new RegistryConfiguration();

            _registryConfigurationBindingSource = new BindingSource
            {
                DataSource = registryConfiguration
            };

            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.Icon_ico,
                ContextMenuStrip = NotifyIconContextMenuStrip(),
            };

            notifyIcon.DoubleClick += NotifyIconOnDoubleClick;

            try
            {
                Run(notifyIcon, registryConfiguration);
            }

            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(ex);
            }

            finally
            {
                notifyIcon.Visible = false;

                registryConfiguration.Dispose();
            }
        }

        public virtual void Run(
            NotifyIcon notifyIcon,
            RegistryConfiguration registryConfiguration)
        {
            if (Deployment.ShouldDownload())
            {
                _updateForm = new UpdateForm(registryConfiguration, notifyIcon, this);

                Application.Run(_updateForm);
            }

            else if (Deployment.ShouldUpdate())
            {
                Update();
            }
        }

        private ContextMenuStrip NotifyIconContextMenuStrip()
        {
            _menuItemRestore = new ToolStripMenuItem(Properties.Resources.MenuItemRestore);

            _menuItemRestore.Click += MenuItemRestoreOnClick;

            var menuItemClose = new ToolStripMenuItem(Properties.Resources.MenuItemClose);

            menuItemClose.Click += MenuItemOnClick;

            var menuItemStartMinimized = new BindableToolStripMenuItem(Properties.Resources.MenuItemStartMinimized)
            {
                CheckOnClick = true
            };

            menuItemStartMinimized.DataBindings.Add(new Binding("Checked", _registryConfigurationBindingSource, "StartMinimized",
                true, DataSourceUpdateMode.OnPropertyChanged));

            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(_menuItemRestore);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(menuItemStartMinimized);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(menuItemClose);
            contextMenuStrip.Opening += ContextMenuStripOnOpening;

            return contextMenuStrip;
        }

        private void ContextMenuStripOnOpening(
            object sender,
            CancelEventArgs cancelEventArgs)
        {
            _menuItemRestore.Text = _updateForm.Visible
                ? Properties.Resources.MenuItemMinimize
                : Properties.Resources.MenuItemRestore;
        }

        private void MenuItemRestoreOnClick(object sender, EventArgs eventArgs)
        {
            _updateForm.ToggleVisivility(FormWindowState.Normal);
        }

        private void NotifyIconOnDoubleClick(
            object sender,
            EventArgs eventArgs)
        {
            _updateForm.ShowAndRestore(FormWindowState.Normal);
        }

        private void MenuItemOnClick(
            object sender,
            EventArgs eventArgs)
        {
            Application.Exit();
        }

        public event DownloadProgressEventHandler DownloadProgress;

        protected void OnDownloadProgress(
            DownloadProgressEventArgs args)
        {
            DownloadProgress?.Invoke(args);
        }

        public event DownloadEventHandler StartDownload;

        protected void OnStartDownload(
            DownloadEventArgs args)
        {
            StartDownload?.Invoke(args);
        }

        public event DownloadEventHandler DownloadCompleted;

        protected void OnDownloadCompleted(
            DownloadEventArgs args)
        {
            DownloadCompleted?.Invoke(args);
        }

        public event UpdateEventHandler UpdateFile;

        protected void OnUpdateFile(
            UpdateEventArgs args)
        {
            UpdateFile?.Invoke(args);
        }

        public event EventHandler StartUpdate;

        protected void OnStartUpdate(
            EventArgs args)
        {
            StartUpdate?.Invoke(this, args);
        }

        public event UpdateExceptionEventHandler UpdateException;

        protected void OnUpdateException(
            ExceptionEventArgs args)
        {
            UpdateException?.Invoke(args);
        }

        public void Start()
        {
            try
            {
                Deployment.SaveTemp();

                OnStartUpdate(new EventArgs());

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
                OnUpdateException(new ExceptionEventArgs
                {
                    Exception = ex
                });
            }
        }

        public void Update()
        {
            if (!Configuration.HasTempDeploymentPath) return;

            var deployment = Deployment.FromTemp(Configuration);

            if (!deployment.UpdateIsPossible()) return;

            deployment.UpdateFile += OnUpdateFile;

            deployment.Update();

            Deployment.SaveTemp();

            DeleteTemp();
        }

        public void UpdateAndExecuteApplication()
        {
            Update();

            if (Configuration.Execution != Execution.None)
            {
                ExecuteApplication();
            }
        }

        protected void ExecuteApplication()
        {
            if (Configuration.SingleInstance)
            {
                var p = PriorApplicationProcess();

                if (p != null)
                {
                    AtivateInstance(p);

                    return;
                }
            }

            if (!Configuration.ApplicationInstalled) return;

            try
            {
                Process.Start(Configuration.ApplicationPath);
            }

            catch
            {
                // Ignore
            }
        }

        public Process PriorApplicationProcess()
        {
            return PriorProcess(Configuration.ApplicationPath);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(
            IntPtr hWnd,
            int nCmdShow);

        private const int KeyeventfExtendedkey = 0x1;
        private const int KeyeventfKeyup = 0x2;
        private const int VkMenu = 0x12;
        private const int Restore = 9;
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(
            byte[] lpKeyState);
        [DllImport("user32.dll")]
        private static extern void keybd_event(
            byte bVk,
            byte bScan,
            uint dwFlags,
            int dwExtraInfo);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(
            IntPtr hWnd);
        public static void SetForegroundWindowInternal(
            IntPtr hWnd)
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

        public void AtivateInstance(
            Process processo)
        {
            if (processo == null) return;

            SetForegroundWindowInternal(processo.MainWindowHandle);
            ShowWindow(processo.MainWindowHandle, Restore);
        }

        public Process PriorProcess(
            string path)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var fileNameToFilter = Path.GetFullPath(path);

            foreach (var p in Process.GetProcesses().Where(w => w.SessionId == currentSessionId))
            {
                try
                {
                    if (string.Compare(fileNameToFilter, p.MainModule.FileName, StringComparison.OrdinalIgnoreCase) == 0)
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
            var files = Directory
                .EnumerateFiles(Configuration.TempDir, "*.*", SearchOption.AllDirectories)
                .Where(file => file != Configuration.TempDeploymentPath);

            foreach (var file in files)
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

            foreach (var dir in Directory.EnumerateDirectories(Configuration.TempDir))
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

        public void ReleaseAssembly(
            string source,
            string destiny,
            List<string> excludedFiles)
        {
            var configuration = Configuration.FromAppSettings();

            var deployment = Deployment.FromAssembly(configuration, source, excludedFiles);

            var build = Path.Combine(destiny, deployment.BuildVersion);

            if (!Directory.Exists(build))
            {
                Directory.CreateDirectory(build);
            }

            deployment.Save(Path.Combine(destiny, "Deployment.xml"));
        }
    }
}
