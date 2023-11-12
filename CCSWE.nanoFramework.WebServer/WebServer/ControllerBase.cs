using System.IO;
using System.Net;
using CCSWE.nanoFramework.Net;

namespace CCSWE.nanoFramework.WebServer
{
    /// <summary>
    /// A base class with helper methods for handling <see cref="HttpListenerResponse"/>.
    /// </summary>
    public abstract class ControllerBase
    {
        /// <summary>
        /// Sets the a status code and writes <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="contentType">The content type.</param>
        /// <remarks>
        /// If <paramref name="body"/> is an <see cref="object"/> it will be serialized as JSON.
        /// If <paramref name="body"/> is a <see cref="byte"/>[] or <see cref="Stream"/> it will written as is.
        /// </remarks>
        protected static void StatusCode(HttpListenerResponse response, HttpStatusCode statusCode, object? body = null, string? contentType = null)
        {
            response.AddCors();
            response.StatusCode = (int)statusCode;

            if (body is null)
            {
                return;
            }

            switch (body)
            {
                case byte[] bytes:
                {
                    response.WriteOutput(bytes, contentType);
                    break;
                }
                case Stream stream:
                {
                    response.WriteOutput(stream, contentType);
                    break;
                }
                default:
                {
                    response.WriteOutput(body, contentType);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the a status code to <see cref="HttpStatusCode.OK"/> and writes <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type.</param>
        /// <remarks>
        /// If <paramref name="body"/> is an <see cref="object"/> it will be serialized as JSON.
        /// If <paramref name="body"/> is a <see cref="byte"/>[] or <see cref="Stream"/> it will written as is.
        /// </remarks>
        protected void Ok(HttpListenerResponse response, object? body = null, string contentType = MimeType.Application.Json) => 
            StatusCode(response, HttpStatusCode.OK, body, contentType); // 200

        /// <summary>
        /// Sets the a status code to <see cref="HttpStatusCode.BadRequest"/> and writes <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type.</param>
        /// <remarks>
        /// If <paramref name="body"/> is an <see cref="object"/> it will be serialized as JSON.
        /// If <paramref name="body"/> is a <see cref="byte"/>[] or <see cref="Stream"/> it will written as is.
        /// </remarks>
        protected void BadRequest(HttpListenerResponse response, object? body = null, string contentType = MimeType.Application.Json) => 
            StatusCode(response, HttpStatusCode.BadRequest, body, contentType); // 400

        /// <summary>
        /// Sets the a status code to <see cref="HttpStatusCode.NotFound"/> and writes <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type.</param>
        /// <remarks>
        /// If <paramref name="body"/> is an <see cref="object"/> it will be serialized as JSON.
        /// If <paramref name="body"/> is a <see cref="byte"/>[] or <see cref="Stream"/> it will written as is.
        /// </remarks>
        protected void NotFound(HttpListenerResponse response, object? body = null, string contentType = MimeType.Application.Json) => 
            StatusCode(response, HttpStatusCode.NotFound, body, contentType); // 404

        /// <summary>
        /// Sets the a status code to <see cref="HttpStatusCode.InternalServerError"/> and writes <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type.</param>
        /// <remarks>
        /// If <paramref name="body"/> is an <see cref="object"/> it will be serialized as JSON.
        /// If <paramref name="body"/> is a <see cref="byte"/>[] or <see cref="Stream"/> it will written as is.
        /// </remarks>
        protected void InternalServerError(HttpListenerResponse response, object? body = null, string contentType = MimeType.Application.Json) => 
            StatusCode(response, HttpStatusCode.InternalServerError, body, contentType); // 500
    }
}
