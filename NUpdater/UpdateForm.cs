using System;
using System.Threading;
using System.Windows.Forms;

namespace NUpdater
{
    public partial class UpdateForm : Form
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Updater _updater;
        private readonly long _totalDownload;
        private long _indexDownload;

        public UpdateForm(NotifyIcon notifyIcon, Updater updater)
        {
            _notifyIcon = notifyIcon;
            _updater = updater;

            notifyIcon.DoubleClick += NotifyIconOnDoubleClick;

            InitializeComponent();

            Text = _updater.Deployment.Configuration.Company + @" - " + string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Name);
            UpdatingAppLabel.Text = string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Name) + @" (" + _updater.Deployment.Version + @")";
            UpdatingMessageLabel.Text = Properties.Resources.UpdateMessage;
            DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, "");
            TotalProgressLabel.Text = Properties.Resources.TotalProgress;

            _updater.StartUpdate += OnUpdaterStart;
            _updater.StartDownload += OnUpdaterStartDownload;
            _updater.DownloadProgress += OnUpdaterDownloadProgress;
            _updater.UpdateFile += OnUpdateFile;

            _totalDownload = _updater.TotalDownload;
        }

        private void OnUpdaterStartDownload(DownloadEventArgs args)
        {
            Invoke(new Action(() => { DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, args.File.Name); }));
        }

        private void OnUpdaterStart(object sender, EventArgs args)
        {
            var message = string.Format(Properties.Resources.NewUpdate, _updater.Deployment.Configuration.Name, _updater.Deployment.Version);

            _notifyIcon.ShowBalloonTip(2000, _updater.Deployment.Configuration.Company, message, ToolTipIcon.Info);
        }

        private void OnUpdaterDownloadProgress(DownloadProgressEventArgs args)
        {
            UpdateWorker.ReportProgress((int) args.Percent, args);
        }

        private void OnUpdateFile(UpdateEventArgs args)
        {
            Invoke(new Action(() => { DownloadingLabel.Text = string.Format(Properties.Resources.UpdateFileProgress, args.File.Name); }));
        }

        private void NotifyIconOnDoubleClick(object sender, EventArgs eventArgs)
        {
            Show();
        }

        private void UpdateWorkerDo(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            _updater.Start();

            var p = _updater.PriorApplicationProcess();

            if (p != null)
            {
                var result = (DialogResult)Invoke(new Func<DialogResult>(() => MessageBox.Show(this, string.Format(Properties.Resources.RestartApplication,
                        _updater.Deployment.Configuration.Path), Properties.Resources.RestartApplicationTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question)));

                if (result == DialogResult.Yes)
                {
                    p.CloseMainWindow();

                    if (!p.WaitForExit(2000))
                    {
                        p.Kill();
                        p.WaitForExit();
                    }

                    _updater.RunApplication();
                }
            }

            else
            {
                _updater.RunApplication();
            }
        }

        private void UpdateWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Application.Exit();
        }

        private void UpdateWorkerProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var args = (DownloadProgressEventArgs) e.UserState;

            progressBar1.Value = (int)args.Percent;

            Interlocked.Add(ref _indexDownload, args.Count);

            var step = (int)(100.0*_indexDownload/_totalDownload);

            progressBar2.Value = step;
            _notifyIcon.Text = string.Format(Properties.Resources.UpdateFileProgress, step + "%");
        }

        private void UpdateFormLoad(object sender, EventArgs e)
        {
            UpdateWorker.RunWorkerAsync();
        }

        private void UpdateFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;

            e.Cancel = true;
            Hide();
        }
    }
}
