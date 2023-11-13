using System;
using CCSWE.nanoFramework.WebServer.Evaluate;
using Microsoft.Extensions.Logging;

namespace CCSWE.nanoFramework.WebServer.Services
{
    /// <inheritdoc />
    internal class WebServerService : IWebServer
    {
        private readonly ILogger _logger;
        private WebServer? _webServer;
        private readonly WebServerOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public WebServerService(ILogger logger, WebServerOptions options, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Type[] controllers = null;

            if (_options.Controllers.Count > 0)
                controllers = (Type[])_options.Controllers.ToArray(typeof(Type));

            _webServer = new WebServer(_options.Port, _options.Protocol, controllers, _logger, _serviceProvider);
        }

        /// <inheritdoc />
        public void Start()
        {
            bool success = _webServer.Start();
            _logger.LogDebug($@"WebServer started: {success}");
        }

        /// <inheritdoc />
        public void Stop()
        {
            _webServer.Stop();
            _logger.LogDebug("WebServer stopped");
        }

        public void Dispose()
        {
            if (_webServer is not null)
            {
                _webServer.Dispose();
            }
        }
    }
}
