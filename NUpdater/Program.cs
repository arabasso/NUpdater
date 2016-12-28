using System;

namespace NUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var updater = new Updater();

            updater.Start();
        }
    }
}
