using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SampleMVCSolution.DataAccessLayer.ExchangeResources
{
    /*
     * These exceptions may be used on higher levels of DAL like repository
     * as well as on lower layers like external resources.
     * This layers shouldn't know about each other (so each of these can be substituted).
     * As a result these exceptions should belong to some neutral namespace
     * which may be used by both of these layers.
     * 
     * This is my apology for creating another entity for keeping these exceptions.
     * Resource entity is not repository as well as it is not ExternalResource but
     * it may be used by both.
    **/
    public class ExchangeResource
    {
        // Case 1: Temporary error, timeout is reached, resource is overloaded or under maintainance, etc.
        public class TryLaterException : BaseException { }
        // Case 2: Permanent error, the request is correct, but impracticable, no such date, change your date.
        public class TryRequestingOtherDatesOrCurrency : BaseException
        {
            public TryRequestingOtherDatesOrCurrency(string message) : base(message) { }
        }
        // Case 3: Maintainance error, the resource is exhausted, payment is over, some usage limit is reached, app_key is blocked.
        // This exception is due to our part.
        public class NotifyResourceMaintainer : BaseException
        {
            public NotifyResourceMaintainer(string messageKeepSecret) : base(messageKeepSecret) { }
            public NotifyResourceMaintainer(string messageKeepSecret, Exception innerException) : base(messageKeepSecret, innerException) { }
        }
        // Case 4: The resource is broken or violates communication protocol.
        // This is exception is mostly due to a resource part internal workings,
        // but also may be a maintainance error (resource has changed it's api) or our code error (we generated a wrong request and got 404).
        // It may be mapped to maintainance exception if the maintainer is interested in all such cases.
        public class ResourceBrokageException : BaseException {
            public ResourceBrokageException(string messageKeepSecret) : base(messageKeepSecret) { }
        }
        // Case 5: There is some unhandled exception while requesting resource 'caused by our code.
        // This exception is better to be wrapped to attach to it a set of failed dates.
        public class NotifyResourceDeveloper : BaseException
        {
            public NotifyResourceDeveloper(Exception innerException) : this("Unexpected exception was caught.", innerException) { }
            public NotifyResourceDeveloper(string message, Exception innerException) : base(message, innerException) { }
        }
        /*
         * Failed dates -- those which were tried to retrive but failed, causing the exception to be thrown.
         * Unprocessed dates -- those which were not tried to be retrieved.
         */
        public class BaseException : Exception
        {
            private IEnumerable<DateTime> untouchedDates = Enumerable.Empty<DateTime>();
            private IEnumerable<DateTime> failedDates = Enumerable.Empty<DateTime>();
            public IEnumerable<DateTime> GetFailedDates()
            {
                return this.failedDates;
            }
            public IEnumerable<DateTime> GetUntouchedDates()
            {
                return this.untouchedDates;
            }
            public void AddFailedDates(IEnumerable<DateTime> failedDates)
            {
                Trace.WriteLine("Adding "+ failedDates.Count() +" failed dates.");
                this.failedDates = this.failedDates.Concat(failedDates);
            }
            public void AddFailedDate(DateTime date)
            {
                this.AddFailedDates(new[] {date});
            }
            public void AddUntouchedDates(IEnumerable<DateTime> failedDates)
            {
                this.untouchedDates = this.untouchedDates.Concat(failedDates);
            }
            
            public BaseException(string message) : base(message) { }
            public BaseException(string message, Exception ex) : base(message, ex) {
                var resEx = ex as BaseException;
                if (resEx != null)
                {
                    this.failedDates = resEx.failedDates; // TODO: may be fixed in tests.
                    this.untouchedDates = resEx.untouchedDates;
                }
            }
            public BaseException() : base() { }
        }
    }
}