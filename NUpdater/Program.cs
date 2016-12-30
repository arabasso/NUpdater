using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        [STAThread]
        static void Main(string [] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var updater = new Updater())
            {
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
                            excludedFiles.AddRange(args[2].Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                        }

                        updater.ReleaseAssembly(source, destiny, excludedFiles);

                        return;
                    }

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
