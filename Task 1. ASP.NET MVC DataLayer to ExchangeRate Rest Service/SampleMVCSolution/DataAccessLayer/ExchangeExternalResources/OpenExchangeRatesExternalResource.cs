using SampleMVCSolution.DataAccessLayer.ExchangeResources.External;
using SampleMVCSolution.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SampleMVCSolution.DataAccessLayer.ExchangeResources
{
    public static class OpenExchangeRatesExternalResource
    {
        private static string AppIdKeepSecret = "4872bf21187e4586b18fb6f4671cec51";

        public static IEnumerable<Exchange> RetrieveExchanges(DateTime dateToRetrieve, string targetCurrencyCode)
        {
            Trace.WriteLine("Call to OpenExchangeRates to retrieve a date: " + dateToRetrieve);
            OpenExchangeRatesServer.Response oerResponse = OpenExchangeRatesServer.GetResponse(dateToRetrieve, AppIdKeepSecret);
            var exchangesOfDate = new List<Exchange>(oerResponse.Rates.Count);
            foreach (var rateKvPair in oerResponse.Rates)
            {
                var exchange = new Exchange() { Date = dateToRetrieve, CurrencyCode = rateKvPair.Key, Rate = rateKvPair.Value };
                exchangesOfDate.Add(exchange);
            }
            return exchangesOfDate;
        }

    }
}