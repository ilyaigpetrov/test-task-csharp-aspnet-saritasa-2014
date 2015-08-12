using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SampleMVCSolution.DataAccessLayer;

namespace SampleMVCSolution
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Trace.WriteLine("============================");
            Trace.WriteLine("Application_Start is called.");

            /*
            Database.SetInitializer(new ExchangeInitializer());
            var context = new ExchangeContext();
            context.Database.Initialize(false);
            */

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            /* Custom Binders */
            var customDateBinder = new DateTimeModelBinder("yyyy-MM-dd");
            ModelBinders.Binders.Add(typeof(DateTime), customDateBinder);
        }
    }

    public class DateTimeModelBinder : DefaultModelBinder
    {
        private readonly string _customFormat;

        public DateTimeModelBinder(string customFormat)
        {
            _customFormat = customFormat;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Trace.WriteLine("CUSTOM model binder is used: BindModel.");
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null)
                Trace.WriteLine("Value is null for: " + bindingContext.ModelName); // F.e., happens when 'DateTime date' controller argument is expected, but route parameter has only {id}. TODO:
            var date = DateTime.ParseExact(value.AttemptedValue, _customFormat, System.Globalization.CultureInfo.InvariantCulture); // Throws exception if not parsable. TODO:
            Trace.WriteLine("HERE IS WHAT I GOT:"+date.ToString());
            return date;
        }
    }
}
