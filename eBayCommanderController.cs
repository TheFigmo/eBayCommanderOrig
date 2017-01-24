using System.Web.Mvc;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using RG.Plugin.eBayCommander.Models;

namespace RG.Plugin.eBayCommander.Controllers
{
    public class eBayCommanderController : BasePluginController
    {
        private readonly eBayCommanderSettings _settings;
        private readonly ILogger _logger;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<Country>  _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository; 
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;

        public eBayCommanderController(eBayCommanderSettings settings, ILogger logger,
            ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IOrderService orderService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService)
        {
            this._settings = settings;
            this._logger = logger;
            this._genericAttributeRepository = genericAttributeRepository;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._eventPublisher = eventPublisher;
            this._genericAttributeService = genericAttributeService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            var model = new ConfigurationModel()
            {
                eBayToken = _settings.eBayToken,
                eBayDefaultStoreId = _settings.eBayDefaultStoreId,
                eBayDefaultProductId = _settings.eBayDefaultProductId
            };

            return View("~/Plugins/RG.Plugin.eBayCommander/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return Content("Access denied");

            if (!ModelState.IsValid)
                return Configure();

            _settingService.ClearCache();   //....clear cache now
            _settings.eBayToken = model.eBayToken;
            _settings.eBayDefaultStoreId = model.eBayDefaultStoreId;
            _settings.eBayDefaultProductId = model.eBayDefaultProductId;
            _settingService.SaveSetting(_settings);
          
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public ActionResult CheckForNewOrders(string returnUrl)
        {           
            // There is already a task that has been created and added to the task scheduler - manually instantiate and execute it now...
            eBayCommanderTask task = new eBayCommanderTask(_logger, _countryRepository, _genericAttributeRepository, _stateProvinceRepository, 
                                                            _countryService, _customerService, _eventPublisher, _genericAttributeService, 
                                                            _productService, _orderService, _settingService, _stateProvinceService);
            task.Execute();
            return Content("Fini!");
        }


        /// <summary>
        /// TODO: Handling requesting and accepting eBay token from within our plugin with no need to visit eBay devlopers site
        /// </summary>
        public ActionResult eBayAuthAccepted(string returnUrl)
        {
            _settings.eBayToken = eBayOrder.FetchToken();
            return Content("Auth Accepted not yet implemented!");
        }

        /// <summary>
        /// TODO: Handling requesting and declining eBay token from within our plugin with no need to visit eBay devlopers site
        /// </summary>
        public ActionResult eBayAuthDeclined(string returnUrl)
        {
            return Content("Auth Declined not yet implemented!");
        }



    }
}