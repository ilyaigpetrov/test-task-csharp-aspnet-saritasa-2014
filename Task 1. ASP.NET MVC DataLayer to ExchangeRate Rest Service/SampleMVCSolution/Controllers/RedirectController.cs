using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleMVCSolution.Controllers
{
    public class RedirectController : Controller
    {
        public new RedirectResult Redirect(string url)
        {
            return RedirectPermanent(url);
        }
    }
}