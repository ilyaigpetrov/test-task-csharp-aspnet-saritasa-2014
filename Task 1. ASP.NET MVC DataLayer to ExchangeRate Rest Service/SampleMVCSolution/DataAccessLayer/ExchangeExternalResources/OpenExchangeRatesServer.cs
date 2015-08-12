using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SampleMVCSolution.DataAccessLayer.ExchangeResources.External
{
    public class OpenExchangeRatesServer
    {
        /*
         * Exceptions:
         *  TryRequestingOtherDatesException
         *      for unavailable dates
         *  NotifyResourceMaintainerException
         *      for resource exhaustion, internal server erros (5xx), responses out of contract
         */
        public static Response GetResponse(DateTime date, string appIdKeepSecret)
        {
            var uri = String.Format(
                "https://openexchangerates.org/api/historical/{0}.json?app_id={1}",
                date.ToString("yyyy-MM-dd"),
                appIdKeepSecret
            );
            Response response;
            try
            {
                try
                {
                    response = GetResponse(uri);
                }
                catch (TimeoutException)
                {
                    var erEx = new ExchangeResource.TryLaterException();
                    throw erEx;
                }
            }
            catch (ExchangeResource.BaseException erEx)
            {
                erEx.AddFailedDate(date);
                throw;
            }
            return response;
        }

        public class Response
        {
            public string Disclaimer { get; set; }
            public string License { get; set; }
            public long Timestamp { get; set; }
            public string Base { get; set; }
            public Dictionary<string, double> Rates { get; set; }
        }

        public class ErrorResponse
        {
            public bool Error { get; set; }
            public int Status { get; set; }
            public string Message { get; set; }
            public string Descriptoin { get; set; }
        }

        private static T DeserializeJsonAs<T>(string jsonString)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Deserialize<T>(jsonString);
        }

        public interface IHttpClient
        {
            HttpResponseMessage Get(string uri);
        }

        public class HttpClientWrapper : IHttpClient
        {
            private readonly HttpClient _client;

            public HttpClientWrapper(HttpClient client)
            {
                _client = client;
            }

            public HttpResponseMessage Get(string uri)
            {
                try
                {
                    return _client.GetAsync(new Uri(uri)).Result;
                }
                catch (AggregateException aEx)
                {
                    foreach (var ex in aEx.InnerExceptions)
                    {
                        if (ex is HttpRequestException)
                        {
                            var rEx = (HttpRequestException)ex;
                            if (rEx.InnerException is WebException)
                            {
                                var wEx = (WebException)rEx.InnerException;
                                Trace.WriteLine("Web Exception with status: " + wEx.Status);
                                // ConnectFailure, NameResolutionFailure, proxy failure, send and receive failures, etc. -> all these may be sorted out and mapped to notify exceptions.
                                throw wEx; // TODO:
                            }
                            throw rEx;
                        }
                        else if (ex is TaskCanceledException)
                        {
                            var tcEx = (TaskCanceledException)ex;
                            // Ok, we don't cancel our tasks, so if this exception happens -- it indicates
                            // the task was canceled internally for some reason.
                            // For sure, one such reason is timeout.
                            // There seems to be no way to distinguish timeout from other reason.
                            // Other reasons for internal cancellation seems to happen rare (like application shutdown?).
                            // So let's take this exception is always being timeout.
                            throw new TimeoutException("Timeout requesting an external resource.");
                        }
                    }
                    throw;
                }
            }
        }

        public static Response GetResponse(string uri, TimeSpan? timeout = null)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (timeout.HasValue)
                    httpClient.Timeout = timeout.Value;
                var wrappedClient = new HttpClientWrapper(httpClient);
                return GetResponse(wrappedClient, uri);
            }
        }

        public static Response GetResponse(IHttpClient httpClient, string uri)
        {
             HttpResponseMessage httpResponse = httpClient.Get(uri);
            int statusCode = (int)httpResponse.StatusCode;
            Trace.WriteLine("Got response with status: " + statusCode);

            var jsonResponse = httpResponse.Content.ReadAsStringAsync().Result;

            if (httpResponse.IsSuccessStatusCode)
            {
                Response oerResponse;
                try
                {
                    oerResponse = DeserializeJsonAs<Response>(jsonResponse);
                    if (oerResponse == null || oerResponse.Rates == null)
                    {
                        Trace.WriteLine("Some required fields were deserialized to null!");
                        throw new Exception("A json string was deserialized to null.");
                    }
                }
                catch (Exception)
                {
                    // A response can't be deserialized into the target Class.
                    throw new ExchangeResource.ResourceBrokageException(
                        "The following can't be deserialized as an expexted successful response:" + jsonResponse
                    );
                }
                Trace.WriteLine("Deserialized the response to an instance.");
                return oerResponse;
            }

            Trace.WriteLine("So, the status is not successful. Let's map it to an error.");

            switch (statusCode)
            {
                case 400: // BadRequest, "not_available" for not available date, "invalid_base" for unsupported base currency
                    throw new ExchangeResource.TryRequestingOtherDatesOrCurrency("Rates on this date are not available.");
                case 401: // Unauthorized, missing_app_id, invalid_app_id, not_allowed
                case 429: // Too Many Requests, access restricted for repeated over-use
                case 403: // Forbidden, for some reasons located at the description field
                    throw new ExchangeResource.NotifyResourceMaintainer(jsonResponse);
                case 404: // Not Found, client requested a non-existent resource, check url
                default:  // 5xx and others unhandled
                    throw new ExchangeResource.ResourceBrokageException(jsonResponse);
            }
        }
    }
}