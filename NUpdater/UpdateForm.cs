using System;
using System.Windows.Forms;

namespace NUpdater
{
    public partial class UpdateForm : Form
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Updater _updater;

        public UpdateForm(NotifyIcon notifyIcon, Updater updater)
        {
            _notifyIcon = notifyIcon;
            _updater = updater;

            InitializeComponent();

            Text = _updater.Deployment.Configuration.Company + @" - " + string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Name);
            UpdatingAppLabel.Text = string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Name) + @" (" + _updater.Deployment.Version + @")";
            UpdatingMessageLabel.Text = Properties.Resources.UpdateMessage;
            DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, "");
            TotalProgressLabel.Text = Properties.Resources.TotalProgress;

            _updater.StartUpdate += (sender, args) =>
            {
                var message = string.Format(Properties.Resources.NewUpdate, _updater.Deployment.Configuration.Name, _updater.Deployment.Version);

                _notifyIcon.ShowBalloonTip(10000, _updater.Deployment.Configuration.Company, message, ToolTipIcon.Info);
            };
            _updater.StartDownload += args =>
            {
                Invoke(new Action(() =>
                {
                    DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, args.File.Name);
                }));
            };
            _updater.DownloadProgress += args => UpdateWorker.ReportProgress((int)args.Percent, args);
            _updater.UpdateFile += args =>
            {
                Invoke(new Action(() =>
                {
                    DownloadingLabel.Text = string.Format(Properties.Resources.UpdateFileProgress, args.File.Name);
                }));
            };

            progressBar2.Maximum = (int)_updater.TotalDownload;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UpdateWorker.RunWorkerAsync();
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
            Close();
        }

        private void UpdateWorkerProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var args = (DownloadProgressEventArgs) e.UserState;

            progressBar1.Value = (int)args.Percent;
            progressBar2.Value += args.Count;
        }
    }
}
