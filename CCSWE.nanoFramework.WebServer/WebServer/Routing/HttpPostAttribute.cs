using System;

namespace CCSWE.nanoFramework.WebServer.Routing
{
    /// <summary>
    /// Identifies an action that supports the HTTP POST method.
    /// </summary>
    public class HttpPostAttribute : HttpMethodAttribute
    {
        private static readonly string[] SupportedMethods = { "POST" };

        /// <summary>
        /// Creates a new <see cref="HttpPostAttribute"/>.
        /// </summary>
        public HttpPostAttribute(): base(SupportedMethods)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpPostAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpPostAttribute(string template): base(SupportedMethods, template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
