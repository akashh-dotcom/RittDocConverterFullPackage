#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R2V2.Infrastructure.Settings;
using RestSharp;

#endregion

namespace R2V2.Core.Cms
{
    public class RestApiBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IContentSettings _contentSettings;
        protected readonly IRestClient RestClient;

        public RestApiBase(IRestClient restClient, IContentSettings contentSettings)
        {
            RestClient = restClient;
            _contentSettings = contentSettings;
        }

        public string ResponseContent { get; set; }

        public virtual IRestResponse Execute(IRestRequest request)
        {
            IRestResponse response = null;
            var stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();
                response = RestClient.Execute(request);
                stopWatch.Stop();

                // CUSTOM CODE: Do more stuff here if you need to...

                return response;
            }
            catch (Exception ex)
            {
                // Handle exceptions in your CUSTOM CODE (restSharp will never throw itself)
                Log.Error($"ERROR - RestApiBase - Exception occurred - Message: {ex.Message}");

                throw;
            }
            finally
            {
                LogRequest(request, response, stopWatch.ElapsedMilliseconds);
            }
        }

        public virtual T Execute<T>(IRestRequest request) where T : new()
        {
            IRestResponse response = null;
            var stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();
                response = RestClient.Execute(request);
                stopWatch.Stop();

                // CUSTOM CODE: Do more stuff here if you need to...

                // We can't use RestSharp deserialization because it could throw, and we need a clean response
                // We need to implement our own deserialization.
                var returnType = JsonConvert.DeserializeObject<T>(response.Content);
                return returnType;
            }
            catch (Exception ex)
            {
                // Handle exceptions in your CUSTOM CODE (restSharp will never throw itself)
                // Handle exceptions in deserialization

                Log.Error($"ERROR - RestApiBase - Exception occurred - Message: {ex.Message}");

                throw;
            }
            finally
            {
                LogRequest(request, response, stopWatch.ElapsedMilliseconds);
            }
        }

        private void LogRequest(IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = RestClient.BuildUri(request)
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage
            };

            Log.Debug(
                $"RestApiBase - Request completed in {durationMs} ms, Request: {JsonConvert.SerializeObject(requestToLog)}, Response: {JsonConvert.SerializeObject(responseToLog)}");
        }


        #region generics

        public T Get<T>(string url) where T : class
        {
            var newUrl = $"{url}&elements.content_requirement__environment[all]={_contentSettings.KenticoEnvironment}";
            Log.Debug(newUrl);
            return CallApi<T>(newUrl, Method.GET);
        }

        public TResult Post<TArg, TResult>(string url, TArg obj)
            where TArg : class
            where TResult : class
        {
            return CallApi<TArg, TResult>(url, Method.POST, obj);
        }

        public T CallApi<T>(string url, Method method) where T : class
        {
            return CallApi<T, T>(url, method);
        }

        public TResult CallApi<TArg, TResult>(string url, Method method, TArg obj = null)
            where TArg : class
            where TResult : class
        {
            var request = new RestRequest(url, method);

            if (obj != null)
            {
                if (method == Method.POST)
                {
                    request.AddJsonBody(obj);
                }
                else
                {
                    request.AddObject(obj);
                }
            }

            TResult result = null;
            var response = Execute(request);
            if (response != null)
            {
                ResponseContent = response.Content;
                if (response.ResponseStatus == ResponseStatus.Completed && (int)response.StatusCode >= 200 &&
                    (int)response.StatusCode < 300)
                {
                    var t = typeof(TResult);
                    JContainer jContainer = JObject.Parse(response.Content);
                    result = jContainer.ToObject<TResult>();
                }
                else
                {
                    const string errorMessage =
                        "ERROR - Call to AORN Web Service failed - Please see log file for full details";
                    Log.Error(errorMessage);
                }
            }
            else
            {
                const string errorMessage = "ERROR - Call to AORN Web Service failed - Response is NULL";
                Log.Error(errorMessage);
            }


            return result;
        }

        #endregion
    }
}