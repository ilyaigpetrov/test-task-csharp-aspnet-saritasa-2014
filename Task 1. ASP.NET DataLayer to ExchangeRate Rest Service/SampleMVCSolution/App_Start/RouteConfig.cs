using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SampleMVCSolution
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Exchanges.Range",
                url: "Exchanges/Range/{startDate}/{endDate}/{targetCurrencyCode}",
                defaults: new
                {
                    controller = "Exchanges",
                    action = "Range",
                }
            );

            routes.MapRoute(
                name: "Exchanges.Range.Root",
                url: "Exchanges/Range",
                defaults: new
                {
                    controller = "Exchanges",
                    action = "RangeRoot",
                }
            );

            routes.MapRoute(
                name: "Exchanges.Default",
                url: "Exchanges/{action}/{date}/{targetCurrencyCode}",
                defaults: new {
                    controller = "Exchanges",
                    action = "Details",
                }
            );

            routes.MapRoute(
                name: "Exchanges.Action.Root",
                url: "Exchanges/{action}",
                defaults: new
                {
                    controller = "Exchanges",
                }
            );

            routes.MapRoute(
                name: "Exchanges.Root",
                url: "Exchanges",
                defaults: new
                {
                    controller = "Exchanges",
                    action = "RangeRoot",
                }
            );

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Redirect", action = "Redirect", url = "Exchanges/" }
            );

        }
    }
}
