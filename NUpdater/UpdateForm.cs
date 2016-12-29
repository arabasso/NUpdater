using System;
using System.Windows.Forms;

namespace NUpdater
{
    public partial class UpdateForm : Form
    {
        private readonly Updater _updater;

        public UpdateForm(Updater updater)
        {
            _updater = updater;

            InitializeComponent();

            Text = string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Path);
            UpdatingAppLabel.Text = string.Format(Properties.Resources.AppUpdate, _updater.Deployment.Configuration.Path);
            UpdatingMessageLabel.Text = Properties.Resources.UpdateMessage;
            DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, "");
            TotalProgressLabel.Text = Properties.Resources.TotalProgress;

            _updater.StartDownload += args =>
            {
                BeginInvoke(new Action(() =>
                {
                    DownloadingLabel.Text = string.Format(Properties.Resources.DownloadFileProgress, args.File.Name);
                }));
            };
            _updater.DownloadProgress += args => UpdateWorker.ReportProgress((int)args.Percent, args);
            _updater.UpdateFile += args =>
            {
                BeginInvoke(new Action(() =>
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
                var message = string.Format(Properties.Resources.RestartApplication,
                    _updater.Deployment.Configuration.Path);

                var result = MessageBox.Show(this, message, Properties.Resources.RestartApplicationTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
