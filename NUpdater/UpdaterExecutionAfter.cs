using System.Windows.Forms;

namespace NUpdater
{
    public class UpdaterExecutionAfter
        : Updater
    {
        public UpdaterExecutionAfter(
            Configuration configuration)
            : base(configuration)
        {
        }

        public override void Run(
            NotifyIcon notifyIcon,
            RegistryConfiguration registryConfiguration)
        {
            base.Run(notifyIcon, registryConfiguration);

            ExecuteApplication();
        }
    }
}
