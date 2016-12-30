using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUpdater
{
    public class DownloadEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
    }

    public delegate void DownloadEventHandler(DownloadEventArgs e);

    public class UpdateEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
    }

    public delegate void UpdateEventHandler(UpdateEventArgs e);

    public class DownloadProgressEventArgs : EventArgs
    {
        public DeploymentFile File { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }
        public float Percent => (float)100.0 * Index / File.Size;
    }

    public delegate void DownloadProgressEventHandler(DownloadProgressEventArgs e);

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }

    public delegate void UpdateExceptionEventHandler(ExceptionEventArgs e);
}
