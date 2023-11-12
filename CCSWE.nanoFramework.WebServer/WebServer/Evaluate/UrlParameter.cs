//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

namespace CCSWE.nanoFramework.WebServer.Evaluate
{
    /// <summary>
    /// Represent an URL parameter Name=Value
    /// </summary>
    public class UrlParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Valeu of the parameter
        /// </summary>
        public string Value { get; set; }
    }
}
