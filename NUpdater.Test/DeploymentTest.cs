using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace NUpdater.Test
{
    [TestFixture]
    public class DeploymentTest
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
        public void From_stream()
        {
            using (var stream = File.OpenRead(@"Remote\App\Deployment.xml"))
            {
                var deployment = Deployment.FromStream(_configuration, stream);

                Assert.That(deployment, Is.Not.Null);
            }
        }

        [Test]
        public void From_uri()
        {
            Assert.That(_deployment, Is.Not.Null);
        }

        [Test]
        public void From_uri_check_address()
        {
            Assert.That(_deployment.Address, Is.EqualTo(new Uri("http://localhost:1234/App/1_0_0_0/")));
        }

        [Test]
        public void From_uri_check_version()
        {
            Assert.That(_deployment.Version, Is.EqualTo(new Version("1.0.0.0")));
        }

        [Test]
        public void From_uri_check_files()
        {
            Assert.That(_deployment.Files, Has.Count.EqualTo(3));
        }

        [Test]
        public void Not_has_update()
        {
            Assert.That(_deployment.ShouldUpdate(), Is.False);
        }

        [Test]
        public void Update_is_possible()
        {
            Assert.That(_deployment.UpdateIsPossible(), Is.True);
        }

        [Test]
        public void Not_update_is_possible()
        {
            var firstFile = _deployment.Files.First();

            using (var stream = File.OpenWrite(firstFile.LocalPath))
            {
                Assert.That(_deployment.UpdateIsPossible(), Is.False);

                stream.Close();
            }
        }
    }
}
