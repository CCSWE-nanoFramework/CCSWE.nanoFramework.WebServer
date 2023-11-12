namespace CCSWE.nanoFramework.WebServer.Routing
{
    /// <summary>
    /// Interface that exposes a list of http methods that are supported by an provider.
    /// </summary>
    public interface IActionHttpMethodProvider
    {
        /// <summary>
        /// The list of http methods this action provider supports.
        /// </summary>
        string[] HttpMethods { get; }
    }
}
