using SampleMVCSolution.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SampleMVCSolution.DataAccessLayer.ExchangeResources
{
    public class ExchangeBackedByExternalResourcesRepository
    {
        private ExchangeContext exchangeContext;
        private DbSet<Exchange> dbSet;

        public ExchangeBackedByExternalResourcesRepository(ExchangeContext exchangeContext)
        {
            this.exchangeContext = exchangeContext;
            this.dbSet = this.exchangeContext.Set<Exchange>();
        }

        public DbSet<Exchange> GetDbSet()
        {
            return this.dbSet;
        }

        public Exchange Remove(DateTime date, string targetCurrencyCode)
        {
            var entity = this.dbSet.Find(date, targetCurrencyCode);
            if (entity != null)
            {
                this.dbSet.Remove(entity);
                return entity;
            }
            return null;
        }

        public IEnumerable<Exchange> GetAll()
        {
            return this.dbSet;
        }

        public void Add(Exchange exchange)
        {
            Trace.WriteLine("Calling Exchange..Repository.Add()");
            this.dbSet.Add(exchange);
        }

        public void Update(Exchange entityToUpdate)
        {
            this.dbSet.Attach(entityToUpdate);
            this.exchangeContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public Exchange Find(DateTime date, string targetCurrencyCode)
        {
            return this.dbSet.Find(date, targetCurrencyCode); // TODO: not found -> hit online, TODO: sql timeout?
        }

        public IEnumerable<Exchange> GetRange(DateTime startDate, DateTime endDate, string targetCurrencyCode)
        {
            var exchangesInRange = this.dbSet
                .Where(
                    e =>
                        startDate <= e.Date && e.Date <= endDate
                )
                .ToList();

            var targetExchanges = exchangesInRange
                .Where(
                    e =>
                        e.CurrencyCode == targetCurrencyCode
                )
                .ToList();

            var numberOfDaysInRange = (endDate - startDate).Days + 1; // Inclusive.

            if (targetExchanges.Count == numberOfDaysInRange)
                return targetExchanges;

            var availableDatesInRange = exchangesInRange.Select(ex => ex.Date).Distinct();
            var availableTargetDates = targetExchanges.Select(ex => ex.Date);
            var rangeCount = availableDatesInRange.Count();
            var targetCount = availableTargetDates.Count();
            if (rangeCount != targetCount)
            {
                Trace.WriteLine("For the range given there are "+rangeCount+" dates in the db, "+targetCount+" of them are for "+targetCurrencyCode+".");
                var resEx = new ExchangeResource.TryRequestingOtherDatesOrCurrency(
                    "Rates for "+targetCurrencyCode+" are not available for some of the dates requested."
                );
                var absentTargetDates = availableDatesInRange.Except(availableTargetDates);
                resEx.AddFailedDates(absentTargetDates);
                throw resEx;
            }


            Trace.WriteLine(
                String.Format("Found rates for {0} out of {1} days in the DB. Let's retrieve the rest!", targetExchanges.Count, numberOfDaysInRange)
            );

            var currentDate = startDate;
            var availableDates = targetExchanges.Select(e => e.Date).ToDictionary(d => d);
            var datesToRetrieve = new List<DateTime>(numberOfDaysInRange - targetExchanges.Count);
            while (currentDate <= endDate) // Inclusive.
            {
                if (!availableDates.ContainsKey(currentDate))
                {
                    datesToRetrieve.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }
            Trace.WriteLine("We have " + datesToRetrieve.Count + " dates to retrieve.");
            IEnumerable<Exchange> retrievedExchanges;
            try
            {
                Trace.WriteLine("Call to exchange external resource.");
                retrievedExchanges = DataAccessLayer.ExchangeResources
                    .External
                    .DefaultExternalResource
                    .RetrieveAndPopulateExchanges(
                        datesToRetrieve,
                        targetCurrencyCode,
                        this.dbSet // TODO: test that if dbSet is populated then it is saved to the db on SaveChanges.
                    );
            }
            finally
            {
                this.exchangeContext.SaveChanges();
            }
            return targetExchanges.Concat(retrievedExchanges.Where(ex => ex.CurrencyCode == targetCurrencyCode));
        }
    }
}