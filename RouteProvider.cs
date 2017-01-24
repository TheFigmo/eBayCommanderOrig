using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace RG.Plugin.eBayCommander
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("RG.Plugin.eBayCommander.CheckForNewOrders", "Plugins/eBayCommander/CheckForNewOrders",
                 new { controller = "eBayCommander", action = "CheckForNewOrders" },
                 new[] { "RG.Plugin.eBayCommander.Controllers" }
            );

            routes.MapRoute("RG.Plugin.eBayCommander.eBayAuthAccepted", "Plugins/eBayCommander/eBayAuthAccepted",
                 new { controller = "eBayCommander", action = "eBayAuthAccepted" },
                 new[] { "RG.Plugin.eBayCommander.Controllers" }
            );

            routes.MapRoute("RG.Plugin.eBayCommander.eBayAuthDeclined", "Plugins/eBayCommander/eBayAuthDeclined",
                 new { controller = "eBayCommander", action = "eBayAuthDeclined" },
                 new[] { "RG.Plugin.eBayCommander.Controllers" }
            );

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
