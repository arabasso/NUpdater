using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUpdater
{
    static class Program
    {
        private static UpdateForm _updateForm;
        private static BindingSource _registryConfigurationBindingSource;

        static Process PriorProcess(Process curr)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var procs = Process.GetProcessesByName(curr.ProcessName);

            return procs.FirstOrDefault(p => (p.SessionId == currentSessionId) && (p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName));
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        [STAThread]
        static void Main(string [] args)
        {
            var registryConfiguration = new RegistryConfiguration();

            _registryConfigurationBindingSource = new BindingSource
            {
                DataSource = registryConfiguration
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.Icon_ico,
                ContextMenuStrip = NotifyIconContextMenuStrip(),
            };

            var updater = new Updater();

            try
            {
                if (args.Length >= 1)
                {
                    if (!AttachConsole(-1))
                    {
                        AllocConsole();
                    }

                    var source = args[0];
                    var destiny = ".";
                    var excludedFiles = new List<string>();

                    if (args.Length >= 2)
                    {
                        destiny = args[1];
                    }

                    if (args.Length >= 3)
                    {
                        excludedFiles.AddRange(args[2].Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries));
                    }

                    updater.ReleaseAssembly(source, destiny, excludedFiles);

                    return;
                }

                if (PriorProcess(Process.GetCurrentProcess()) != null) return;

                var task = Run(updater, notifyIcon, registryConfiguration);

                if (task.Result)
                {
                    Application.Run();
                }
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

        private static Task<bool> Run(Updater updater, NotifyIcon notifyIcon, RegistryConfiguration registryConfiguration)
        {
            return Task.Factory.StartNew(param =>
            {
                var cfg = (RegistryConfiguration) param;

                updater.RunApplication();

                if (!updater.ShouldDownload()) return false;

                _updateForm = new UpdateForm(cfg, notifyIcon, updater)
                {
                    Visible = !cfg.StartMinimized
                };

                return true;
            }, registryConfiguration, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static ContextMenuStrip NotifyIconContextMenuStrip()
        {
            var menuItemRestore = new ToolStripMenuItem(Properties.Resources.MenuItemRestore);

            menuItemRestore.Click += MenuItemRestoreOnClick;

            var menuItemClose = new ToolStripMenuItem(Properties.Resources.MenuItemClose);

            menuItemClose.Click += MenuItemOnClick;

            var menuItemStartMinimized = new BindableToolStripMenuItem(Properties.Resources.MenuItemStartMinimized)
            {
                CheckOnClick = true
            };

            menuItemStartMinimized.DataBindings.Add(new Binding("Checked", _registryConfigurationBindingSource, "StartMinimized",
                true, DataSourceUpdateMode.OnPropertyChanged));

            var contextMenuStrip = new ContextMenuStrip();

            contextMenuStrip.Items.Add(menuItemRestore);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(menuItemStartMinimized);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(menuItemClose);

            return contextMenuStrip;
        }

        private static void MenuItemRestoreOnClick(object sender, EventArgs eventArgs)
        {
            _updateForm.Show();
        }

        private static void MenuItemOnClick(object sender, EventArgs eventArgs)
        {
            Application.Exit();
        }
    }
}
