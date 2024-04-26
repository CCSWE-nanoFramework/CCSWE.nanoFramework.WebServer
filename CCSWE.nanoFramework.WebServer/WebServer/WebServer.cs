//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CCSWE.nanoFramework.Threading.Internal;
using CCSWE.nanoFramework.WebServer.Evaluate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthenticationType = CCSWE.nanoFramework.WebServer.Evaluate.AuthenticationType;

namespace CCSWE.nanoFramework.WebServer
{
    /// <summary>
    /// This class instantiates a web server.
    /// </summary>
    public class WebServer : IDisposable
    {
        /// <summary>
        /// URL parameter separation character
        /// </summary>
        public const char ParamSeparator = '&';

        /// <summary>
        /// URL parameter start character
        /// </summary>
        public const char ParamStart = '?';

        /// <summary>
        /// URL parameter equal character
        /// </summary>
        public const char ParamEqual = '=';

        #region internal objects

        private bool _cancel;
        private Thread? _serverThread;
        private readonly ArrayList _callbackRoutes;
        private readonly HttpListener _listener;
        private readonly ThreadPoolInternal _threadPool;

        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the port the server listens on.
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// The type of Http protocol used, http or https
        /// </summary>
        public HttpProtocol Protocol { get; protected set; }

        /// <summary>
        /// The Https certificate to use
        /// </summary>
        public X509Certificate HttpsCert
        {
            get => _listener.HttpsCert;

            set => _listener.HttpsCert = value;
        }

        /// <summary>
        /// SSL protocols
        /// </summary>
        public SslProtocols SslProtocols
        {
            get => _listener.SslProtocols;

            set => _listener.SslProtocols = value;
        }

        /// <summary>
        /// Network credential used for default user:password couple during basic authentication
        /// </summary>
        public NetworkCredential? Credential { get; set; }

        /// <summary>
        /// Default APiKey to be used for authentication when no key is specified in the attribute
        /// </summary>
        public string? ApiKey { get; set; }

        #endregion

        #region Param

        /// <summary>
        /// Get an array of parameters from a URL
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static UrlParameter[] DecodeParam(string parameter)
        {
            UrlParameter[] retParams = null;
            int i = parameter.IndexOf(ParamStart);
            int k;

            if (i >= 0)
            {
                while (i < parameter.Length || i == -1)
                {
                    int j = parameter.IndexOf(ParamEqual, i);
                    if (j > i)
                    {
                        //first param!
                        if (retParams == null)
                        {
                            retParams = new UrlParameter[1];
                            retParams[0] = new UrlParameter();
                        }
                        else
                        {
                            UrlParameter[] rettempParams = new UrlParameter[retParams.Length + 1];
                            retParams.CopyTo(rettempParams, 0);
                            rettempParams[rettempParams.Length - 1] = new UrlParameter();
                            retParams = new UrlParameter[rettempParams.Length];
                            rettempParams.CopyTo(retParams, 0);
                        }
                        k = parameter.IndexOf(ParamSeparator, j);
                        retParams[retParams.Length - 1].Name = parameter.Substring(i + 1, j - i - 1);
                        // Nothing at the end
                        if (k == j)
                        {
                            retParams[retParams.Length - 1].Value = "";
                        }
                        // Normal case
                        else if (k > j)
                        {
                            retParams[retParams.Length - 1].Value = parameter.Substring(j + 1, k - j - 1);
                        }
                        // We're at the end
                        else
                        {
                            retParams[retParams.Length - 1].Value = parameter.Substring(j + 1, parameter.Length - j - 1);
                        }
                        if (k > 0)
                            i = parameter.IndexOf(ParamSeparator, k);
                        else
                            i = parameter.Length;
                    }
                    else
                        i = -1;
                }
            }
            return retParams;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>
        public WebServer(int port, HttpProtocol protocol, ILogger logger, IServiceProvider serviceProvider) : this(port, protocol, null, logger, serviceProvider)
        { }

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>
        /// <param name="controllers">Controllers to use with this web server.</param>
        public WebServer(int port, HttpProtocol protocol, Type[]? controllers, ILogger logger, IServiceProvider serviceProvider)
        {
            _callbackRoutes = new ArrayList();
            _logger = logger;
            _serviceProvider = serviceProvider;
            _threadPool = new ThreadPoolInternal(32, 32);
            _threadPool.SetMinThreads(4);

            AddControllersOld(controllers);

            Protocol = protocol;
            Port = port;
            string prefix = Protocol == HttpProtocol.Http ? "http" : "https";
            _listener = new HttpListener(prefix, port);
            _logger.LogDebug("Web server started on port " + port.ToString());
        }

        private void AddControllers(Type[]? controllers)
        {
            if (controllers is null)
            {
                return;
            }


        }

        private void AddControllersOld(Type[]? controllers)
        {
            if (controllers != null)
            {
                foreach (var controller in controllers)
                {
                    Authentication? authentication = null;

                    var controllerAttributes = controller.GetCustomAttributes(true);
                    foreach (var controllerAttribute in controllerAttributes)
                    {
                        if (controllerAttribute is not AuthenticationAttribute authenticationAttribute)
                        {
                            continue;
                        }

                        authentication = ExtractAuthentication(authenticationAttribute.AuthenticationMethod);
                    }

                    var methods = controller.GetMethods();
                    foreach (var method in methods)
                    {
                        RouteCallback? routeCallback = null;

                        var methodAttributes = method.GetCustomAttributes(true);
                        foreach (var methodAttribute in methodAttributes)
                        {
                            if (typeof(RouteAttribute) == methodAttribute.GetType())
                            {
                                routeCallback = new RouteCallback();
                                routeCallback.Route = ((RouteAttribute)methodAttribute).Template;
                                routeCallback.CaseSensitive = false;
                                routeCallback.Method = string.Empty;
                                routeCallback.Authentication = authentication;

                                // The Trim fixes routes with a trailing slash from failing
                                routeCallback.RouteParts = routeCallback.Route.Trim('/').Split('/');
                                if (routeCallback.Route.Contains("{"))
                                {
                                    var routeParams = new ArrayList();
                                    for (int i = 0; i < routeCallback.RouteParts.Length; i++)
                                    {
                                        if (routeCallback.RouteParts[i][0] == '{')
                                            routeParams.Add(i);
                                    }
                                    routeCallback.RouteParamsIndexes = (int[])routeParams.ToArray(typeof(int));
                                }

                                routeCallback.Callback = method;
                                foreach (var otherattrib in methodAttributes)
                                {
                                    if (typeof(MethodAttribute) == otherattrib.GetType())
                                    {
                                        routeCallback.Method = ((MethodAttribute)otherattrib).Method;
                                    }
                                    else if (typeof(CaseSensitiveAttribute) == otherattrib.GetType())
                                    {
                                        routeCallback.CaseSensitive = true;
                                    }
                                    else if (typeof(AuthenticationAttribute) == otherattrib.GetType())
                                    {
                                        var strAuth = ((AuthenticationAttribute)otherattrib).AuthenticationMethod;
                                        // A method can have a different authentication than the main class, so we override if any
                                        routeCallback.Authentication = ExtractAuthentication(strAuth);
                                    }
                                }

                                _callbackRoutes.Add(routeCallback);
                                _logger.LogTrace($"{routeCallback.Callback.Name}, {routeCallback.Route}, {routeCallback.Method}, {routeCallback.CaseSensitive}");
                                // _logger.LogTrace($"{routeCallback.Callback.Name}, {routeCallback.Route.EscapeForInterpolation()}, {routeCallback.Method}, {routeCallback.CaseSensitive}");
                            }
                        }
                    }

                }
            }
        }


        private Authentication ExtractAuthentication(string strAuth)
        {
            const string _none = "None";
            const string _basic = "Basic";
            const string _apiKey = "ApiKey";

            Authentication authentication;
            if (strAuth.IndexOf(_none) == 0)
            {
                if (strAuth.Length == _none.Length)
                {
                    authentication = new Authentication();
                }
                else
                {
                    throw new ArgumentException($"Authentication attribute None can only be used alone");
                }
            }
            else if (strAuth.IndexOf(_basic) == 0)
            {
                if (strAuth.Length == _basic.Length)
                {
                    authentication = new Authentication((NetworkCredential)null);
                }
                else
                {
                    var sep = strAuth.IndexOf(':');
                    if (sep == _basic.Length)
                    {
                        var space = strAuth.IndexOf(' ');
                        if (space < 0)
                        {
                            throw new ArgumentException($"Authentication attribute Basic should be 'Basic:user passowrd'");
                        }

                        var user = strAuth.Substring(sep + 1, space - sep - 1);
                        var password = strAuth.Substring(space + 1);
                        authentication = new Authentication(new NetworkCredential(user, password, System.Net.AuthenticationType.Basic));
                    }
                    else
                    {
                        throw new ArgumentException($"Authentication attribute Basic should be 'Basic:user passowrd'");
                    }
                }
            }
            else if (strAuth.IndexOf(_apiKey) == 0)
            {
                if (strAuth.Length == _apiKey.Length)
                {
                    authentication = new Authentication(string.Empty);
                }
                else
                {
                    var sep = strAuth.IndexOf(':');
                    if (sep == _apiKey.Length)
                    {
                        var key = strAuth.Substring(sep + 1);
                        authentication = new Authentication(key);
                    }
                    else
                    {
                        throw new ArgumentException($"Authentication attribute ApiKey should be 'ApiKey:thekey'");
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Authentication attribute can only start with Basic, None or ApiKey and case sensitive");
            }
            return authentication;
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate for the CommandReceived event.
        /// </summary>
        public delegate void GetRequestHandler(object obj, WebServerEventArgs e);

        /// <summary>
        /// CommandReceived event is triggered when a valid command (plus parameters) is received.
        /// Valid commands are defined in the AllowedCommands property.
        /// </summary>
        public event GetRequestHandler? CommandReceived;

        #endregion

        #region Public and private methods

        /// <summary>
        /// Start the multi threaded server.
        /// </summary>
        public bool Start()
        {
            if (_serverThread == null)
            {
                _serverThread = new Thread(StartListener);
            }

            bool bStarted = true;
            // List Ethernet interfaces, so we can determine the server's address
            ListInterfaces();
            // start server           
            try
            {
                _cancel = false;
                _serverThread.Start();
                _logger.LogDebug("Started server in thread " + _serverThread.GetHashCode().ToString());
            }
            catch
            {   //if there is a problem, maybe due to the fact we did not wait enough
                _cancel = true;
                bStarted = false;
            }
            return bStarted;
        }

        /// <summary>
        /// Restart the server.
        /// </summary>
        private bool Restart()
        {
            Stop();
            return Start();
        }

        /// <summary>
        /// Stop the multi threaded server.
        /// </summary>
        public void Stop()
        {
            _cancel = true;
            
            Thread.Sleep(100);

            _serverThread?.Abort();
            _serverThread = null;
            
            _logger.LogDebug("Stoped server in thread ");
        }

        private void StartListener()
        {
            _listener.Start();

            while (!_cancel)
            {
                var context = _listener.GetContext();
                if (context is null)
                {
                    _logger.LogError($"{nameof(StartListener)}: Context is null. Why?");
                    return;
                }

                _threadPool.QueueUserWorkItem(HandleRequest, context);

            }

            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }

        /// <summary>
        /// Method which invokes route. Can be overriden to inject custom logic.
        /// </summary>
        /// <param name="route">Current rounte to invoke. Resolved based on parameters.</param>
        /// <param name="callbackParams"></param>
        protected virtual void InvokeRoute(RouteCallback route, object[] callbackParams)
        {
            _logger.LogTrace($"Invoking {route.Callback.DeclaringType}.{route.Callback.Name}");
            var dp = ActivatorUtilities.CreateInstance(_serviceProvider, route.Callback.DeclaringType);
            route.Callback.Invoke(dp, callbackParams);
        }

        private string GetApiKeyFromHeaders(WebHeaderCollection headers)
        {
            var sec = headers.GetValues("ApiKey");
            if (sec != null
                && sec.Length > 0)
            {
                return sec[0];
            }

            return null;
        }

        /// <summary>
        /// List all IP address, useful for debug only
        /// </summary>
        private void ListInterfaces()
        {
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
            _logger.LogDebug("Number of Interfaces: " + ifaces.Length.ToString());
            foreach (NetworkInterface iface in ifaces)
            {
                _logger.LogDebug("IP:  " + iface.IPv4Address + "/" + iface.IPv4SubnetMask);
            }
        }

        /// <summary>
        /// Dispose of any resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing">Dispose of resources?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serverThread = null;
            }
        }

        protected virtual bool IsRouteMatch(RouteCallback route, HttpListenerContext context, out object[] callbackParams)
        {
            callbackParams = null;

            if (context.Request.HttpMethod != route.Method)
                return false;

            var urlAndParams = context.Request.RawUrl.Split('?');
            string url = urlAndParams[0];
            url = route.CaseSensitive ? url : url.ToLower();

            var urlParts = url.Trim('/').Split('/');

            if (urlParts.Length != route.RouteParts.Length)
                return false;

            bool isMatch = true;
            callbackParams = new object[route.RouteParamsIndexes == null ? 1 : route.RouteParamsIndexes.Length + 1];
            int paramIndex = 0;
            for (int i = 0; i < route.RouteParts.Length; i++)
            {
                if (route.RouteParamsIndexes != null && i == route.RouteParamsIndexes[paramIndex])
                {
                    callbackParams[paramIndex] = urlParts[i];
                    paramIndex++;
                }
                else if (route.RouteParts[i] != urlParts[i])
                {
                    isMatch = false;
                    break;
                }
            }

            if (!isMatch)
                return false;

            callbackParams[callbackParams.Length - 1] = new WebServerEventArgs(context);

            return true;
        }

        #endregion


        // **** REFACTORED BELOW HERE ****

        private void HandleRequest(object? state)
        {
            if (state is not HttpListenerContext context)
            {
                _logger.LogError($"{nameof(HandleRequest)}: {nameof(state)} was null");
                return;
            }

            HandleRequest(context);
        }

        // TODO: In progress
        private void HandleRequest(HttpListenerContext context)
        {
            bool isRoute = false;
            string rawUrl = context.Request.RawUrl;

            //This is for handling with transitory or bad requests
            if (rawUrl == null)
            {
                return;
            }


            // Variables used only within the "for". They are here for performance reasons
            bool mustAuthenticate;
            bool isAuthOk;
            //

            foreach (var rt in _callbackRoutes)
            {
                RouteCallback route = (RouteCallback)rt;

                if (!IsRouteMatch(route, context, out var callbackParams))
                    continue;

                // Starting a new thread to be able to handle a new request in parallel
                isRoute = true;

                // Check auth first
                mustAuthenticate = route.Authentication != null && route.Authentication.AuthenticationType != AuthenticationType.None;
                isAuthOk = !mustAuthenticate;

                if (mustAuthenticate)
                {
                    if (route.Authentication.AuthenticationType == AuthenticationType.Basic)
                    {
                        var credSite = route.Authentication.Credentials ?? Credential;
                        var credReq = context.Request.Credentials;

                        isAuthOk = credReq != null
                            && credSite.UserName == credReq.UserName
                            && credSite.Password == credReq.Password;
                    }
                    else if (route.Authentication.AuthenticationType == AuthenticationType.ApiKey)
                    {
                        var apikeySite = route.Authentication.ApiKey ?? ApiKey;
                        var apikeyReq = GetApiKeyFromHeaders(context.Request.Headers);

                        isAuthOk = apikeyReq != null
                            && apikeyReq == apikeySite;
                    }
                }

                if (!isAuthOk)
                {
                    if (route.Authentication != null && route.Authentication.AuthenticationType == AuthenticationType.Basic)
                    {
                        context.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"Access to {route.Route}\"");
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentLength64 = 0;
                    _logger.LogTrace($"Response code: {context.Response.StatusCode}");
                    return;
                }

                InvokeRoute(route, callbackParams);

                if (context.Response != null)
                {
                    _logger.LogTrace($"Response code: {context.Response.StatusCode}");
                    context.Response.Close();
                    context.Close();
                    break;
                }
            }

            if (!isRoute)
            {
                if (CommandReceived != null)
                {
                    // Starting a new thread to be able to handle a new request in parallel
                    CommandReceived.Invoke(this, new WebServerEventArgs(context));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentLength64 = 0;
                }

                // When context has been handed over to WebsocketServer, it will be null at this point
                if (context.Response == null)
                {
                    //do nothing this is a websocket that is managed by a websocketserver that is responsible for the context now. 
                }
                else
                {
                    _logger.LogTrace($"Response code: {context.Response.StatusCode}");
                    context.Response.Close();
                    context.Close();
                }
            }
        }
    }
}
