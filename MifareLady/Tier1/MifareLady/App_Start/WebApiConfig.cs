using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;

namespace MifareLady
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            JsonSerializerSettings serializerSettings = config.Formatters.JsonFormatter.SerializerSettings;
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
        }
    }
}
