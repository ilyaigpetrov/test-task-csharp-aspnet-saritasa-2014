using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleMVCSolution.DataAccessLayer.ExchangeResources;
using SampleMVCSolution.DataAccessLayer.ExchangeResources.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace SampleMVCSolution.Tests.DataLayerAccess.ExchangeResources.External
{
    [TestClass]
    public class OpenExchangeRatesServerTest
    {

        class HttpClientMock : OpenExchangeRatesServer.IHttpClient
        {
            private HttpStatusCode statusCode;
            private StringContent responseStringContent;

            public HttpClientMock(int statusCode, string responseMessage)
            {
                this.statusCode = (HttpStatusCode)statusCode;
                this.responseStringContent = new StringContent(responseMessage);
            }

            public HttpResponseMessage Get(string uri)
            {
                var response = new HttpResponseMessage(this.statusCode);
                response.Content = this.responseStringContent;
                return response;
            }
        }

        private static void InvokeGetResponseWith(int statusCode, string responseMessage = "This is a default test message.")
        {
            OpenExchangeRatesServer.IHttpClient client = new HttpClientMock(statusCode, responseMessage);
            OpenExchangeRatesServer.GetResponse(client, "http://forTesting");
        }

        private static ExType AssertThrows<ExType>(Action statement, string message) where ExType : Exception
        {
            try
            {
                statement();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ExType), message);
                return (ExType)ex;
            }
            Assert.Fail(message);
            return null;
        }

        [TestMethod]
        public void GetResponse_Responses_401_429_403_Throw_Appropriate_Exception()
        {
            var responseCodes = new int[] { 401, 429, 403 };
            foreach (var code in responseCodes)
            {
                AssertThrows<ExchangeResource.NotifyResourceMaintainer>(
                        () => InvokeGetResponseWith(code),
                        "The code "+code+" doesn't map to an appropriate exception."
                );
            }
        }

        [TestMethod]
        public void GetResponse_Response_200_With_NonDeserializable_Content_Throws_Appropriate_Exception()
        {
            var responseMessages = new string[] {
                "<Test: How would you handle a non-json response?>",
                "{ message: 'something more like json but still out of format' }"
            };
            foreach (var message in responseMessages)
            {
                AssertThrows<ExchangeResource.ResourceBrokageException>(
                        () => InvokeGetResponseWith(200, message),
                        "The following message was deserialized but mustn't be: "+message
                );
            }
        }

        [TestMethod]
        [ExpectedException(typeof(WebException), "TODO: currently dns failre is not mapped, so just check it throws something.")]
        public void GetResponse_DNS_Failure_Throws_Appropriate_Exception()
        {
            OpenExchangeRatesServer.GetResponse("http://nonExistantName.nonExistant");
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException), "TODO: decide what type of exception should be thrown.")]
        public void GetResponse_Timeout_Throws_Appropriate_Exception()
        {
            OpenExchangeRatesServer.GetResponse("http://localhost", new TimeSpan(1));
        }


    }
}