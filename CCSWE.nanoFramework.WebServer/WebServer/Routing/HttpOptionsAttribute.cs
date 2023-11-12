using System;

namespace CCSWE.nanoFramework.WebServer.Routing
{
    /// <summary>
    /// Identifies an action that supports the HTTP OPTIONS method.
    /// </summary>
    public class HttpOptionsAttribute : HttpMethodAttribute
    {
        private static readonly string[] SupportedMethods = { "OPTIONS" };

        /// <summary>
        /// Creates a new <see cref="HttpOptionsAttribute"/>.
        /// </summary>
        public HttpOptionsAttribute(): base(SupportedMethods)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpOptionsAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpOptionsAttribute(string template): base(SupportedMethods, template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
