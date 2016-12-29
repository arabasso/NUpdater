using System;
using System.Diagnostics;
using System.Linq;
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
            if (PriorProcess(Process.GetCurrentProcess()) != null) return;

            var updater = new Updater();

            updater.RunApplication();

            if (!updater.ShouldDownload()) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateForm(updater));
        }
    }
}
