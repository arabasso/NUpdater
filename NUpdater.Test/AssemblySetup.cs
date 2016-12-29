using System.IO;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUpdater.Test
{
    [SetUpFixture]
    public class AssemblySetup
    {
        private HttpListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:1234/");
            _listener.Start();

            Task.Factory.StartNew(() =>
            {
                while (_listener.IsListening)
                {
                    try
                    {
                        Handle(_listener.GetContext());
                    }

                    catch
                    {
                        // Ignore
                    }
                }
            });
        }

        void Handle(HttpListenerContext ctx)
        {
            var path = Path.Combine("Remote", ctx.Request.Url.AbsolutePath.Substring(1));

            var response = ctx.Response;

            if (!File.Exists(path))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusDescription = "File not found";
                response.OutputStream.Close();

                return;
            }

            using (var fs = File.OpenRead(path))
            {
                var filename = Path.GetFileName(path);

                response.ContentLength64 = fs.Length;
                response.SendChunked = false;
                response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                var buffer = new byte[64 * 1024];

                using (var bw = new BinaryWriter(response.OutputStream))
                {
                    int read;

                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        bw.Flush();
                    }

                    bw.Close();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.OutputStream.Close();
            }

            response.Close();
        }

        [TearDown]
        public void TearDown()
        {
            _listener.Stop();
            _listener.Abort();
        }
    }
}
