using CCSWE.nanoFramework.WebServer.Evaluate.Services;
using CCSWE.nanoFramework.WebServer.Evaluate;
using Microsoft.Extensions.DependencyInjection;

namespace CCSWE.nanoFramework.WebServer
{
    /// <summary>
    /// Extension methods for <see cref="WebServer"/>.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Adds an <see cref="WebServer"/> with the specified <see cref="WebServerOptions"/>.
        /// </summary>
        public static IServiceCollection AddWebServer(this IServiceCollection services, ConfigureWebServerOptions? configureOptions = null)
        {
            services.AddSingleton(typeof(IServer), typeof(Server));
            var options = new WebServerOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton(typeof(WebServerOptions), options);

            return services;
        }
    }
}
