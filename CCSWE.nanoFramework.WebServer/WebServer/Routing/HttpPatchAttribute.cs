using System;

namespace CCSWE.nanoFramework.WebServer.Routing
{
    /// <summary>
    /// Identifies an action that supports the HTTP PATCH method.
    /// </summary>
    public class HttpPatchAttribute : HttpMethodAttribute
    {
        private static readonly string[] SupportedMethods = { "PATCH" };

        /// <summary>
        /// Creates a new <see cref="HttpPatchAttribute"/>.
        /// </summary>
        public HttpPatchAttribute(): base(SupportedMethods)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpPatchAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpPatchAttribute(string template): base(SupportedMethods, template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
