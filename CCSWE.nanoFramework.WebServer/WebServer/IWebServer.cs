using System;

namespace CCSWE.nanoFramework.WebServer
{
    // TODO: Check docs... Implement in concrete WebServer?

    /// <summary>
    /// Web server with Controllers support
    /// </summary>
    public interface IWebServer : IDisposable
    {
        /// <summary>
        /// Starts web server.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops web server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Initializes web server, register Controllers etc.
        /// </summary>
        void Initialize();
    }
}