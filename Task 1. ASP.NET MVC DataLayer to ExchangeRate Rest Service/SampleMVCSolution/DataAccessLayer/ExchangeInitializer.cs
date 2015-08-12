using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SampleMVCSolution.Models;
using System.Diagnostics;

namespace SampleMVCSolution.DataAccessLayer
{
    public class ExchangeInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ExchangeContext>
    {
        public ExchangeInitializer() : base()
        {
            Trace.WriteLine("ExchangeInitializer is created.");
            System.Data.SqlClient.SqlConnection.ClearAllPools();
        }

        protected override void Seed(ExchangeContext context)
        {
            Trace.WriteLine("ExchangeInitializer.Seed is called.");
            var exchangeList = new List<Exchange>
            {
                new Exchange{Date = DateTime.Parse("2005-09-01"), CurrencyCode = "USD", Rate = 1.0},
                new Exchange{Date = DateTime.Parse("2005-09-02"), CurrencyCode = "USD", Rate = 2.0},
                new Exchange{Date = DateTime.Parse("2005-09-03"), CurrencyCode = "USD", Rate = 3.0},
            };

            exchangeList.ForEach(r => context.ExchangeRepository.Add(r));
            context.SaveChanges();
        }
    }
}