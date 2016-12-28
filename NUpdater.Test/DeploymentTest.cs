using System;
using System.Net;
using System.Net.Cache;
using NUnit.Framework;

namespace NUpdater.Test
{
    [TestFixture]
    public class DeploymentTest
    {
        [Test]
        public void From_stream()
        {
            var request = WebRequest.Create("http://localhost:1234/Deployment.xml");

            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    var deployment = Deployment.FromStream(stream);

                    Assert.That(deployment, Is.Not.Null);
                }
            }
        }

        [Test]
        public void From_uri()
        {
            var deployment = Deployment.FromUri(new Uri("http://localhost:1234/Deployment.xml"));

            Assert.That(deployment, Is.Not.Null);
        }
    }
}
