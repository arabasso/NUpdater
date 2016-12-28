using System;
using System.IO;
using NUnit.Framework;

namespace NUpdater.Test
{
    [TestFixture]
    public class DeploymentTest
    {
        private Uri _uri;
        private Deployment _deployment;

        [SetUp]
        public void Set_up()
        {
            _uri = new Uri("http://localhost:1234/App/Deployment.xml");
            _deployment = Deployment.FromUri(_uri);
        }

        [Test]
        public void From_stream()
        {
            using (var stream = File.OpenRead(@"Remote\App\Deployment.xml"))
            {
                var deployment = Deployment.FromStream(stream);

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
            Assert.That(_deployment.Address, Is.EqualTo("http://localhost:1234/App/1_0_0_0/"));
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
    }
}
