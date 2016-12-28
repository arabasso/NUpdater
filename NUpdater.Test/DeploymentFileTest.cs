using System;
using System.IO;
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
        public void Not_should_update()
        {
            var file = _deployment.Files.First();

            Assert.That(file.ShouldUpdate(), Is.False);
        }

        [Test]
        public void Should_update()
        {
            _deployment.Configuration.Path = "App2";

            var file = _deployment.Files.First();

            Assert.That(file.ShouldUpdate(), Is.True);
        }

        [Test]
        public void Update_file()
        {
            _deployment.Configuration.Path = "App2";

            var file = _deployment.Files.First();

            file.Update();

            Assert.That(file.HasLocal, Is.True);

            File.Delete(file.LocalPath);
        }

        [Test]
        public void Temp_path()
        {
            var file = _deployment.Files.First();

            Assert.That(file.TempPath, Is.EqualTo(Environment.ExpandEnvironmentVariables(@"%TEMP%\NUpdater\App\App.exe")));
        }

        [Test]
        public void Local_path()
        {
            var file = _deployment.Files.First();

            Assert.That(file.LocalPath, Is.EqualTo(@"App\App.exe"));
        }

        [Test]
        public void has_temp()
        {
            var file = _deployment.Files.First();

            file.Download();

            Assert.That(file.HasTemp, Is.True);

            file.DeleteTemp();
        }

        [Test]
        public void has_local()
        {
            var file = _deployment.Files.First();

            Assert.That(file.HasLocal, Is.True);
        }

        [Test]
        public void Delete_temp_path()
        {
            var file = _deployment.Files.First();

            file.Download();

            Assert.That(file.HasTemp, Is.True);

            file.DeleteTemp();

            Assert.That(file.HasTemp, Is.False);
        }

        [Test]
        public void Not_downloaded()
        {
            var file = _deployment.Files.Last();

            Assert.That(file.HasTemp, Is.False);
        }
    }
}
