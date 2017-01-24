using System.Web.Routing;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace RG.Plugin.eBayCommander
{
    public class eBayCommanderPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public eBayCommanderPlugin(ISettingService settingService, 
            IScheduleTaskService scheduleTaskService)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        public void GetConfigurationRoute(out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "eBayCommander";
            routeValues = new RouteValueDictionary() { { "Namespaces", "RG.Plugin.eBayCommander.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new eBayCommanderSettings
            {
                eBayToken = "",
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayToken", "eBay API Application Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayToken.Hint", "Enter your eBay Application Key here. You must first sign up for the eBay developer's program to be assigned this key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayRequestKey", "Request API Token From eBay");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayDefaultStoreId", "Default Store Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayDefaultStoreId.Hint", "The nopCommerce StoreId of the store that new eBay orders will be added to");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayDefaultProductId", "Default Product Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayDefaultProductId.Hint", "The ProductId to use if the SKU of an eBay product cannot be found in nopCommerce");
            this.AddOrUpdatePluginLocaleResource("Plugins.eBayCommander.eBayCheckForNewOrders", "Check for new eBay orders now");

            _scheduleTaskService.InsertTask(new ScheduleTask()
            {
                Enabled = true,
                Name = "eBayCommander - Check for New Orders",
                Seconds = 600,
                StopOnError = false,
                Type = "RG.Plugin.eBayCommander.eBayCommanderTask, RG.Plugin.eBayCommander"
            });

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<eBayCommanderSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayToken");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayToken.Hint");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayRequestKey");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayDefaultStoreId");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayDefaultStoreId.Hint");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayDefaultProductId");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayDefaultProductId.Hint");
            this.DeletePluginLocaleResource("Plugins.eBayCommander.eBayCheckForNewOrders");

            ScheduleTask task = _scheduleTaskService.GetTaskByType("RG.Plugin.eBayCommander.eBayCommanderTask, RG.Plugin.eBayCommander");
            if (task != null)
                _scheduleTaskService.DeleteTask(task);

            base.Uninstall();
        }

        #endregion
    }
}