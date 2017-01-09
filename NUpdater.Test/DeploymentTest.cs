using System;
using System.Collections.Generic;
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
        public void Not_update_is_possible()
        {
            var firstFile = _deployment.Files.First();

            using (var stream = File.Open(firstFile.LocalPath, FileMode.Create))
            {
                Assert.That(_deployment.UpdateIsPossible(), Is.False);

                stream.Close();
            }
        }

        [Test]
        public void From_assembly_check_version()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Version, Is.EqualTo(new Version("1.0.0.0")));
        }

        [Test]
        public void From_assembly_check_version_build()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.BuildVersion, Is.EqualTo("1_0_0_0"));
        }

        [Test]
        public void From_assembly_check_address()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Address, Is.EqualTo(new Uri("http://localhost:1234/App/1_0_0_0/")));
        }

        [Test]
        public void From_assembly_check_file_count()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Files, Has.Count.EqualTo(3));
        }

        [Test]
        public void From_assembly_check_file_names()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Files[0].Name, Is.EqualTo("App.exe"));
            Assert.That(d.Files[1].Name, Is.EqualTo("App.exe.config"));
            Assert.That(d.Files[2].Name, Is.EqualTo(@"Source\Program.cs"));
        }

        [Test]
        public void From_assembly_check_file_sizes()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Files[0].Size, Is.EqualTo(4608));
            Assert.That(d.Files[1].Size, Is.EqualTo(77));
            Assert.That(d.Files[2].Size, Is.EqualTo(235));
        }

        [Test]
        public void From_assembly_check_file_hashes()
        {
            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Files[0].Hash, Is.EqualTo("32fe50ad52b26d8afad1e26d8af2a614"));
            Assert.That(d.Files[1].Hash, Is.EqualTo("feb8a12f54cdbca11133449147e40b28"));
            Assert.That(d.Files[2].Hash, Is.EqualTo("8458173506da806f2a7b5916177ddebf"));
        }

        [TestCase(@"Source\Program.cs")]
        [TestCase(@"*.cs")]
        [TestCase(@"Source\Program.*")]
        [TestCase(@"Source\Program.*")]
        public void From_assembly_excluded_files(string wildcard)
        {
            var excludedList = new List<string> { wildcard };

            var d = Deployment.FromAssembly(_configuration, "App", excludedList);

            Assert.That(d.Files, Has.Count.EqualTo(2));
        }

        [TestCase(@"App\App.vshost.exe")]
        [TestCase(@"App\App.vshost.exe.config")]
        [TestCase(@"App\App.vshost.exe.manifest")]
        public void From_assembly_excluded_extension(string file)
        {
            using (File.Open(file, FileMode.Create))
            {
            }

            var d = Deployment.FromAssembly(_configuration, "App");

            Assert.That(d.Files, Has.Count.EqualTo(3));

            File.Delete(file);
        }
    }
}
