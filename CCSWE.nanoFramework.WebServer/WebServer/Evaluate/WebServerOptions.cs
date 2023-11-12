using System;
using System.Collections;

namespace CCSWE.nanoFramework.WebServer.Evaluate
{
    public class WebServerOptions
    {
        internal readonly ArrayList Controllers = new ArrayList();

        public int Port { get; set; } = 80;
        public HttpProtocol Protocol { get; set; } = HttpProtocol.Http;

        public void AddController(Type controllerType)
        {
            Controllers.Add(controllerType);
        }
    }
}
