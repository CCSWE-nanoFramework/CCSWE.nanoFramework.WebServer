﻿using System;
using System.Net.Security;
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

            if (_options.Certificate is not null)
            {
                _webServer.HttpsCert = _options.Certificate;
                _webServer.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            }
        }

        /// <inheritdoc />
        public void Start()
        {
            var started = _webServer is not null && _webServer.Start();
            
            _logger.LogDebug($@"WebServer started: {started}");
        }

        /// <inheritdoc />
        public void Stop()
        {
            // Nullable<T> is not supported
            if (_webServer is not null)
            {
                _webServer.Stop();
            }

            _logger.LogDebug("WebServer stopped");
        }

        public void Dispose()
        {
            if (_webServer is not null)
            {
                // Nullable<T> is not supported
                _webServer.Dispose();
            }
        }
    }
}
