using System.Net;

namespace CCSWE.nanoFramework.Net
{
    /// <summary>
    /// Extension methods for <see cref="HttpListenerResponse"/>
    /// </summary>
    public static class HttpListenerResponseExtensions
    {
        /// <summary>
        /// Add CORS headers to the <see cref="HttpListenerResponse"/>.
        /// </summary>
        public static void AddCors(this HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
        }
    }
}
