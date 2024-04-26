using System;
using CCSWE.nanoFramework.WebServer.Evaluate;
using CCSWE.nanoFramework.WebServer.Routing;

namespace CCSWE.nanoFramework.WebServer.Reflection
{
    internal static class ReflectionHelper
    {
        private static readonly Type AuthenticationAttributeType = typeof(AuthenticationAttribute);
        private static readonly Type HttpMethodProviderType = typeof(IHttpMethodProvider);
        private static readonly Type RouteTemplateProviderType = typeof(IRouteTemplateProvider);

        public static bool IsHttpMethodProvider(object? value)
        {
            return value is not null && IsHttpMethodProvider(value.GetType());
        }

        public static bool IsHttpMethodProvider(Type type)
        {
            var interfaces = type.GetInterfaces();
            foreach (var current in interfaces)
            {
                if (current == HttpMethodProviderType)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsRouteTemplateProvider(object? value)
        {
            return value is not null && IsRouteTemplateProvider(value.GetType());
        }

        public static bool IsRouteTemplateProvider(Type type)
        {
            var interfaces = type.GetInterfaces();
            foreach (var current in interfaces)
            {
                if (current == RouteTemplateProviderType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
