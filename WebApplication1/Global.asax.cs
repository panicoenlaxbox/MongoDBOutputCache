﻿using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MongoDBOutputCache;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureOuputCache();
        }

        private static void ConfigureOuputCache()
        {
            OutputCacheAttribute.ChildActionCache = new MongoDBChildActionCache("My cache");
        }
    }
}