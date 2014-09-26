using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.UI;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 60, Location = OutputCacheLocation.Server, VaryByParam = "*")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

         [OutputCache(Duration = 60, VaryByParam = "*")]
        public PartialViewResult Information()
        {
            return PartialView();
        }

        [OutputCache(Duration = 60, Location = OutputCacheLocation.Server, VaryByParam = "*")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            Response.RemoveOutputCacheItem(Url.Action("Information"));

            return View();
        }
    }
}