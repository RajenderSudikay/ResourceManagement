using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ResourceManagement
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
             "IT", "IT/{action}/{name}", new
             {
                 controller = "IT",
                 action = "Index",
                 name = UrlParameter.Optional
             });

            routes.MapRoute(
           "MasterData", "MasterData/{action}/{name}", new
           {
               controller = "MasterData",
               action = "Index",
               name = UrlParameter.Optional
           });

            //routes.MapRoute(
            // "assetaddupdate", "asset-addupdate/{name}", new
            // {
            //     controller = "Asset",
            //     action = "AddUpdate",
            //     name = UrlParameter.Optional
            // });

            routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );           

        }
    }
}
