using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NUpdater
{
    static class Program
    {
        public static bool PriorProcess(Process curr)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var procs = Process.GetProcessesByName(curr.ProcessName);

            return procs.Any(p => (p.SessionId == currentSessionId) && (p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName));
        }

        [STAThread]
        static void Main()
        {
            var notify = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.Icon32x32
            };

            try
            {
                if (PriorProcess(Process.GetCurrentProcess())) return;

                var configuration = Configuration.FromAppSettings();

                if (configuration.ApplicationInstalled)
                {
                    configuration.RunApplication();
                }

                var deployment = Deployment.FromUri(configuration);

                if (!deployment.ShouldUpdate()) return;

                notify.ShowBalloonTip(10000, "", string.Format(Properties.Resources.NewUpdate, configuration.Path), ToolTipIcon.Info);

                foreach (var file in deployment.Files.Where(file => !file.HasTemp))
                {
                    file.Download();
                }

                if (!deployment.UpdateIsPossible()) return;

                foreach (var file in deployment.Files.Where(w => !w.IsLocked))
                {
                    file.Update();
                }

                configuration.DeleteTemp();

                if (configuration.ApplicationInstalled)
                {
                    configuration.RunApplication();
                }

                else
                {
                    notify.ShowBalloonTip(10000, "", string.Format(Properties.Resources.SucessUpdate, configuration.Path), ToolTipIcon.Info);
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
    }
}
