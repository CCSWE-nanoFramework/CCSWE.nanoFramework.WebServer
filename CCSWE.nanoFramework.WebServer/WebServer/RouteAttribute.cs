using System;
using CCSWE.nanoFramework.WebServer.Routing;

namespace CCSWE.nanoFramework.WebServer
{
    // TODO: Allow routes on the class for specifying base path

    /// <summary>
    /// Specifies an attribute route on a controller.
    /// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute, IRouteTemplateProvider
    {
        /// <summary>
        /// Creates a new <see cref="RouteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public RouteAttribute(string template)
        {
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <inheritdoc />
        public string Template { get; }
    }
}
