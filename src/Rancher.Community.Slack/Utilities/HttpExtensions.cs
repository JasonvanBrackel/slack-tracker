using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Rancher.Community.Slack.Utilities
{
        public static class HttpExtensions
        {
            private static readonly JsonSerializer Serializer = new JsonSerializer();

            public static Task WriteJson<T>(this HttpResponse response, T obj)
            {
                response.ContentType = "application/json";
                return response.WriteAsync(JsonConvert.SerializeObject(obj));
            }

            public static async Task<T> ReadFromJson<T>(this HttpContext httpContext)
            {
                using (var streamReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var obj = Serializer.Deserialize<T>(jsonTextReader);

                    var results = new List<ValidationResult>();
                    if (Validator.TryValidateObject(obj, new ValidationContext(obj), results))
                    {
                        httpContext.Request.Body.Position = 0L;
                        return obj;
                    }

                    httpContext.Request.Body.Position = 0L;
                    httpContext.Response.StatusCode = 400;
                    await httpContext.Response.WriteJson(results);

                    return default(T);
                }
            }

            public static T ReadFromJson<T>(this HttpResponseMessage response)
            {
                using (var streamReader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var obj = Serializer.Deserialize<T>(jsonTextReader);

                    Validator.ValidateObject(obj, new ValidationContext(obj));
                    return obj;                    
                }
                
            }

            public static string ReadAsString(this HttpRequest request)
            {
                using (var streamReader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var body = streamReader.ReadToEnd();
                    request.Body.Position = 0L;
                    return body;
                }
            }

            public static string ReadAsString(this HttpResponseMessage response)
            {
                var result = response.Content.ReadAsStreamAsync().Result;
                using (var streamReader = new StreamReader(result, Encoding.UTF8, true, 1024, true))
                {
                    var body = streamReader.ReadToEnd();
                    result.Position = 0L;
                    return body;
                }
            }
        }
    }
