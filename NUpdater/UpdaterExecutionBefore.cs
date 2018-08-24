using System.Windows.Forms;

namespace NUpdater
{
    public class UpdaterExecutionBefore
        : Updater
    {
        public UpdaterExecutionBefore(
            Configuration configuration)
            : base(configuration)
        {
        }

        public override void Run(
            NotifyIcon notifyIcon,
            RegistryConfiguration registryConfiguration)
        {
            if (!Deployment.ShouldDownload())
            {
                Update();
            }

            var tempDeployment = Deployment.FromTemp(Configuration);

            if (tempDeployment.IsValid())
            {
                ExecuteApplication();

                base.Run(notifyIcon, registryConfiguration);
            }

            else
            {
                base.Run(notifyIcon, registryConfiguration);

                ExecuteApplication();
            }
        }
    }
}
