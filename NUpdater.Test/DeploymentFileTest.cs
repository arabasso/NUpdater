using System;
using System.Linq;
using NUnit.Framework;

namespace NUpdater.Test
{
    [TestFixture]
    public class DeploymentFileTest
    {
        private Configuration _configuration;
        private Deployment _deployment;

        [SetUp]
        public void Set_up()
        {
            _configuration = Configuration.FromAppSettings();
            _deployment = Deployment.FromUri(_configuration);
        }

        [Test]
        public void Not_downloaded_not_should_update()
        {
            var file = _deployment.Files.First();

            Assert.That(file.ShouldUpdate(), Is.False);
        }

        [Test]
        public void Downloaded_but_not_should_update()
        {
            var file = _deployment.Files.First();

            file.Download();

            Assert.That(file.ShouldUpdate(), Is.False);

            file.DeleteTempPath();
        }

        [Test]
        public void Temp_path()
        {
            var file = _deployment.Files.First();

            Assert.That(file.TempPath, Is.EqualTo(Environment.ExpandEnvironmentVariables(@"%TEMP%\NUpdater\App\App.exe")));
        }

        [Test]
        public void Was_downloaded()
        {
            var file = _deployment.Files.First();

            file.Download();

            Assert.That(file.WasDownloaded, Is.True);

            file.DeleteTempPath();
        }

        [Test]
        public void Delete_temp_path()
        {
            var file = _deployment.Files.First();

            file.Download();

            Assert.That(file.WasDownloaded, Is.True);

            file.DeleteTempPath();

            Assert.That(file.WasDownloaded, Is.False);
        }

        [Test]
        public void Not_downloaded()
        {
            var file = _deployment.Files.Last();

            Assert.That(file.WasDownloaded, Is.False);
        }
    }
}
