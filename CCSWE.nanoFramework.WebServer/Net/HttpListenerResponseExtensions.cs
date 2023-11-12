using nanoFramework.Json;
using System.IO;
using System.Net;
using System.Text;

namespace CCSWE.nanoFramework.Net
{
    /// <summary>
    /// Extension methods for <see cref="HttpListenerResponse"/>
    /// </summary>
    public static class HttpListenerResponseExtensions
    {
        private const int BufferSize = 1024;

        /// <summary>
        /// Add CORS headers to the <see cref="HttpListenerResponse"/>.
        /// </summary>
        public static void AddCors(this HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
        }

        /// <summary>
        /// Serializes <paramref name="body"/> to JSON and writes it to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type. Defaults to <see cref="MimeType.Application.Json"/>.</param>
        public static void WriteOutput(this HttpListenerResponse response, object? body, string? contentType = null)
        {
            if (body is null)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(body as string ?? JsonConvert.SerializeObject(body));

            response.WriteOutput(bytes, contentType ?? MimeType.Application.Json);
        }

        /// <summary>
        /// Writes the <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type. Defaults to <see cref="MimeType.Application.Json"/>.</param>
        public static void WriteOutput(this HttpListenerResponse response, byte[] body, string? contentType = null)
        {
            response.ContentLength64 = body.Length;
            response.ContentType = contentType;
            response.SendChunked = response.ContentLength64 > BufferSize; // Is this good?

            for (var bytesSent = 0L; bytesSent < body.Length;)
            {
                var bytesToSend = body.Length - bytesSent;
                bytesToSend = bytesToSend < BufferSize ? bytesToSend : BufferSize;

                response.OutputStream.Write(body, (int)bytesSent, (int)bytesToSend);
                bytesSent += bytesToSend;
            }
        }

        /// <summary>
        /// Writes the <paramref name="body"/> to the output stream.
        /// </summary>
        /// <param name="response">The <see cref="HttpListenerResponse"/> to write to.</param>
        /// <param name="body">The body content.</param>
        /// <param name="contentType">The content type. Defaults to <see cref="MimeType.Application.Octet"/>.</param>
        public static void WriteOutput(this HttpListenerResponse response, Stream body, string? contentType = null)
        {
            response.ContentLength64 = body.Length;
            response.ContentType = contentType ?? MimeType.Application.Octet;
            response.SendChunked = response.ContentLength64 > BufferSize; // Is this good?

            var buffer = new byte[BufferSize];
            var bytesSent = 0L;
            int bytesToSend;

            while ((bytesToSend = body.Read(buffer)) > 0)
            {
                response.OutputStream.Write(buffer, (int)bytesSent, bytesToSend);
                bytesSent += bytesToSend;
            }
        }
    }
}
