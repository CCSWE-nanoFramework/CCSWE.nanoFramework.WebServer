using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace CCSWE.nanoFramework.WebServer.Evaluate
{
    public class WebServerOptions
    {
        internal readonly ArrayList Controllers = new();

        public X509Certificate? Certificate { get; set; }
        public int Port { get; set; } = 80;
        public HttpProtocol Protocol { get; set; } = HttpProtocol.Http;

        public void AddController(Type controllerType)
        {
            Controllers.Add(controllerType);
        }
    }
}
