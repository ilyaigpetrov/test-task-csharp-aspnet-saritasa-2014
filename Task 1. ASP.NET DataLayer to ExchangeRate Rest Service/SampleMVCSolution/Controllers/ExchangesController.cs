using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SampleMVCSolution.DataAccessLayer;
using SampleMVCSolution.DataAccessLayer.ExchangeResources;
using SampleMVCSolution.Models;
using System.Diagnostics;


namespace SampleMVCSolution.Controllers
{
    public class ExchangesController : Controller
    {
        private ExchangeContext exchangeContext = new ExchangeContext();

        // For debug purposes.
        public ActionResult Index()
        {
            Trace.WriteLine("INDEX");
            return View(exchangeContext.ExchangeRepository.GetAll());
        }

        public ActionResult RangeRoot()
        {
            Trace.WriteLine("RANGE W/O arguments.");
            var endDate = DateTime.Now;
            var startDate = DateTime.Now.Subtract(TimeSpan.FromDays(14));
            var currencyCode = "RUB";
            var model = new ExchangeRangeViewModel { StartDate = startDate, EndDate = endDate, TargetCurrencyCode = currencyCode };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RangeRoot(ExchangeRangeViewModel exchangeRange)
        {
            Trace.WriteLine("RangeRoot for exchangeRange.");
            if (ModelState.IsValid)
            {
                Trace.WriteLine("Model is valid. Be ready to request the range.");
                return RedirectToAction("Range", exchangeRange.ToRouteValues());
            }
            Trace.WriteLine("Model is not valid.");
            return View(exchangeRange);
        }

        /*
         * Returns rates for a given currencyCode from startDate to endDate inclusive.
         * GET: Exchanges/Range/2014-02-25/2014-02-27/RUB
         **/
        public ActionResult Range(ExchangeRangeViewModel exchangeRange)
        {
            Trace.WriteLine("RANGE for "+exchangeRange+".");
            var startDate = exchangeRange.StartDate;
            var endDate = exchangeRange.EndDate;
            var targetCurrencyCode = exchangeRange.TargetCurrencyCode;

            var maxNumberOfDaysInRange = 62;
            Trace.WriteLine(
                String.Format(
                    "Range requested for: {0}-{1} {2}",
                    startDate, endDate, targetCurrencyCode
                )
            );
            var numberOfDaysInRange = (endDate - startDate).Days + 1; // Inclusive.
            if (!(0 < numberOfDaysInRange && numberOfDaysInRange <= maxNumberOfDaysInRange))
            {
                return new HttpStatusCodeResult(HttpStatusCode.RequestedRangeNotSatisfiable,
                    String.Format("Number of days in a range must be positive and less than {0}.", maxNumberOfDaysInRange));
            }
            IEnumerable<Exchange> exchanges;
            try
            {
                exchanges = exchangeContext.ExchangeRepository.GetRange(startDate, endDate, targetCurrencyCode);
                Trace.WriteLine("Retrieved range of responses without errors.");
            }
            catch (ExchangeResource.BaseException resEx)
            {
                Trace.WriteLine("ExchangeResource exception caught with "+resEx.GetFailedDates().Count()+" failed dates. Specifying type...");
                var failedDatesString = String.Join(", ", resEx.GetFailedDates().Select(d => d.ToString("yyyy-MM-dd")));
                var header = String.Format(
                    "Couldn't supply information about {0} on these dates: {1}. Reason:<br/>",
                    targetCurrencyCode,
                    failedDatesString
                );
                header = header.Replace("\r", "").Replace("\n", "<br/>"); // Linebreaks are not allowed in http headers.
                try {
                    throw;
                }
                catch (ExchangeResource.TryLaterException)
                {
                    return new HttpStatusCodeResult(
                        HttpStatusCode.ServiceUnavailable,
                        header + "Temporary fail. The application has temporary difficulties with specified currency and dates. Try a bit later."
                    );
                }
                catch (ExchangeResource.TryRequestingOtherDatesOrCurrency requestEx)
                {
                    return new HttpStatusCodeResult(
                        HttpStatusCode.NotFound,
                        header + "The application has no information about this currency on dates requested."
                        + "<br/>" +requestEx.Message
                    );
                }
                catch (ExchangeResource.BaseException)
                {
                    Trace.WriteLine("Some base resource exception caught. Header for response: <"+header+">");
                    return new HttpStatusCodeResult(
                        HttpStatusCode.ServiceUnavailable,
                        header + "The application can't complete your request for specified currency and dates due to some internal conflicts."
                    );
                }
            }
            Trace.WriteLine("Now we have all exchanges and ready to render a view.");
            return View(exchanges);
        }

        // GET: Exchanges/Details/2014-09-28/RUB
        public ActionResult Details(DateTime date, string targetCurrencyCode)
        {
            Trace.WriteLine("DETAILS:" + date +" for " + targetCurrencyCode + ".");
            Exchange exchange = exchangeContext.ExchangeRepository.Find(date, targetCurrencyCode);
            if (exchange == null)
            {
                return HttpNotFound();
            }
            return View(exchange);
        }

        /*
        Delete is dangerous, it may put current backed-up repository in an inconsistent state, see README.
        public ActionResult Delete(DateTime date, string targetCurrencyCode)
        {
            Trace.WriteLine("DELETE:" + date + " for " + targetCurrencyCode + ".");
            var removedEntity = exchangeContext.ExchangeRepository.Remove(date, targetCurrencyCode);
            if (removedEntity == null)
                return HttpNotFound();
            exchangeContext.SaveChanges();
            return View(removedEntity);
        }
        */

        public ActionResult About()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                exchangeContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
