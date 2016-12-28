using System;
using NUnit.Framework;

namespace NUpdater.Test
{
    [TestFixture]
    public class ConfigurationTest
    {
        private Configuration _configuration;

        [SetUp]
        public void Set_up()
        {
            _configuration = Configuration.FromAppSettings();
        }

        [Test]
        public void Check_address()
        {
            Assert.That(_configuration.Address, Is.EqualTo(new Uri("http://localhost:1234/App/Deployment.xml")));
        }

        [Test]
        public void Check_path()
        {
            Assert.That(_configuration.Path, Is.EqualTo("App"));
        }

        [Test]
        public void Check_anonymous_proxy()
        {
            Assert.That(_configuration.AnonymousProxy, Is.True);
        }

        [Test]
        public void Check_executable()
        {
            Assert.That(_configuration.Executable, Is.EqualTo("App.exe"));
        }

        [Test]
        public void Check_proxy_address()
        {
            Assert.That(_configuration.ProxyAddress, Is.EqualTo("http://192.168.1.1:3128"));
        }

        [Test]
        public void Check_proxy_enable()
        {
            Assert.That(_configuration.ProxyEnable, Is.False);
        }

        [Test]
        public void Check_proxy_user()
        {
            Assert.That(_configuration.ProxyUser, Is.EqualTo("user"));
        }

        [Test]
        public void Check_proxy_pass()
        {
            Assert.That(_configuration.ProxyPassword, Is.EqualTo("pass"));
        }

        [Test]
        public void Application_installed()
        {
            Assert.That(_configuration.ApplicationInstalled, Is.True);
        }
    }
}
