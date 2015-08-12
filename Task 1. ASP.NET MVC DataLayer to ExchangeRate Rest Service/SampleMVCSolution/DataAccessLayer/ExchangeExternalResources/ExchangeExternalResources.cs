using SampleMVCSolution.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SampleMVCSolution.DataAccessLayer.ExchangeResources.External
{
    /*
        Class exposes `RetrieveAndPopulateExchanges` static method
        which uses external sources like web sites to retrieve information,
        construct Exchanges and populate DbSet.
    **/
    public static class DefaultExternalResource
    {
        public static IEnumerable<Exchange> RetrieveAndPopulateExchanges(IEnumerable<DateTime> datesToRetrieve, string targetCurrencyCode, DbSet<Exchange> exchangesDbSet)
        {
            var retrievedExchanges = Enumerable.Empty<Exchange>();
            foreach (var dateToProcess in datesToRetrieve)
            {
                IEnumerable<Exchange> exchangesOfDate;
                try
                {
                    try
                    {
                        exchangesOfDate = RequestDefaultExternalResource(dateToProcess, targetCurrencyCode);
                    }
                    catch (ExchangeResource.ResourceBrokageException rbEx)
                    {
                        throw new ExchangeResource.NotifyResourceMaintainer("The resource is broken or violates protocol.", rbEx);
                    }
                    catch (ExchangeResource.BaseException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var erEx = new ExchangeResource.NotifyResourceDeveloper(ex);
                        erEx.AddFailedDate(dateToProcess);
                        throw erEx;
                    }
                }
                catch (ExchangeResource.BaseException resEx)
                {
                    var triedDates = retrievedExchanges.Select(e => e.Date).Concat(resEx.GetFailedDates());
                    var untouchedDates = datesToRetrieve.Except(triedDates);
                    resEx.AddUntouchedDates(untouchedDates);
                    throw;
                }
                foreach (var exchange in exchangesOfDate)
                    exchangesDbSet.Add(exchange); // TODO: dangerous if given exchange is in the DB already. `SaveChanges` will fail (and drop other changes?).
                Trace.WriteLine("Database is populated with "+exchangesOfDate.Count()+" entries for date "+dateToProcess.ToString("yyyy-MM-dd")+".");
                retrievedExchanges = retrievedExchanges.Concat(exchangesOfDate);
                var targetExchanges = exchangesOfDate.Where(ex => ex.CurrencyCode == targetCurrencyCode);
                if (targetExchanges.Count() == 0)
                {
                    var resEx = new ExchangeResource.TryRequestingOtherDatesOrCurrency(
                        String.Format(
                            "No rates for currency code {0} on {1}.",
                            targetCurrencyCode,
                            dateToProcess.ToString("yyyy-MM-dd")
                        )
                    );
                    resEx.AddFailedDate(dateToProcess);
                    throw resEx;
                }
            }
            return retrievedExchanges;
        }

        private static IEnumerable<Exchange> RequestDefaultExternalResource(DateTime date, string targetCurrencyCode)
        {
            return OpenExchangeRatesExternalResource.RetrieveExchanges(date, targetCurrencyCode);
        }
    }
}