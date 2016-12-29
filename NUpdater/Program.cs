using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NUpdater
{
    static class Program
    {
        static Process PriorProcess(Process curr)
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;

            var procs = Process.GetProcessesByName(curr.ProcessName);

            return procs.FirstOrDefault(p => (p.SessionId == currentSessionId) && (p.Id != curr.Id) && (p.MainModule.FileName == curr.MainModule.FileName));
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var updater = new Updater())
            {
                try
                {
                    if (PriorProcess(Process.GetCurrentProcess()) != null) return;

                    updater.RunApplication();

                    if (!updater.ShouldDownload()) return;

                    Application.Run(new UpdateForm(updater));
                }

                catch (Exception ex)
                {
                    updater.Log(ex);
                }
            }
        }
    }
}
