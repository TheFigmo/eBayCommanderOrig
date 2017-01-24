using System;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;



namespace RG.Plugin.eBayCommander
{
    /// <summary>
    /// Represents a task for deleting guest customers
    /// </summary>
    public partial class eBayCommanderTask : ITask
    {
        private readonly eBayCommanderSettings _settings;
        private readonly ILogger _logger;

        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;

        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;

        public eBayCommanderTask(ILogger logger,
            IRepository<Country> countryRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<StateProvince> stateProvinceRepository,
            ICountryService countryService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IProductService productService,
            IOrderService orderService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService)
        {
            this._logger = logger;

            this._countryRepository = countryRepository;
            this._genericAttributeRepository = genericAttributeRepository;
            this._stateProvinceRepository = stateProvinceRepository;

            this._countryService = countryService;
            this._customerService = customerService;
            this._eventPublisher = eventPublisher;
            this._genericAttributeService = genericAttributeService;
            this._orderService = orderService;
            this._productService = productService;
            this._settingService = settingService;
            this._stateProvinceService = stateProvinceService;

            _settings = _settingService.LoadSetting<eBayCommanderSettings>();
        }

        /// <summary>
        /// Executes a task
        /// Get all eBay orders, see if they are already in nopCommerce and if not - import them
        /// </summary>
        public void Execute()
        {
            eBayOrder.Init(_settings.eBayToken);    //...init eBay API using token from settings (warning: not authenticated until used below)

            // Step through all recent eBay Orders...
            // TODO: We assume this task will execute every 'x' minutes so we only look for new orders in the past 24 hours but a smaller number would increase speed.  Should this be a setting? 
            foreach (eBayOrder ordEBay in eBayOrder.GetOrdersByDate(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow))
            {
                if (ordEBay.PaymentStatus != "Complete")
                    continue; //...skip any eBay orders where payment is not complete

                if (eBayOrderAlreadyProcessed(ordEBay.OrderId))
                    continue; //...skip any eBay orders that have already been imported into nopCommerce

                // Import eBay order into nopCommerce...
                Order ordNop = eBayOrderImport(ordEBay);
                
                _eventPublisher.Publish(new OrderPaidEvent(ordNop));    //...fire OrderPaidEvent (so other plugins are made aware of this new order)
            }
        }

        /// <summary>
        /// Imports eBay order into nopCommerce store specified in settings.  
        /// If eBay customer email matches existing nopCommerce customer - that customer is used - else new "guest" customer is added and used
        /// If SKU of eBay item ordered matches existing nopCommerce product - that product is used - else default nopCommerce product is used (specified in settings)
        /// NOTE: Orders in nopCommerce must have both Billing and Shipping address.  Since eBay only has a Shipping address, 
        ///       we leave the Customer Billing Address null - but for Orders, we just use the Shipping address for both
        /// </summary>
        /// <param name="ordEBay">eBay order object</param>
        private Order eBayOrderImport(eBayOrder ordEBay)
        {
            var eBayCommanderSettings = _settingService.LoadSetting<eBayCommanderSettings>();

            Customer cust = eBayCustomerAddOrUpdate(ordEBay);   //...convert eBay customer info to nopCommerce customer

            // Add new order to nopCommerce...
            Order ordNop = new Order();
            ordNop.OrderGuid = Guid.NewGuid();
            ordNop.CustomerId = cust.Id;
            ordNop.StoreId = eBayCommanderSettings.eBayDefaultStoreId;
            ordNop.OrderStatusId = 20;
            ordNop.ShippingStatusId = 20;
            ordNop.BillingAddressId = cust.BillingAddress?.Id ?? cust.ShippingAddress.Id;
            ordNop.ShippingAddressId = cust.ShippingAddress?.Id ?? -1;
            ordNop.CreatedOnUtc = ordEBay.OrderDate.ToUniversalTime();

            ordNop.CustomerCurrencyCode = "USD";
            ordNop.PaymentStatusId = 30; //...PaymentStatus.Paid
            switch (ordEBay.PaymentMethod)
            {
                case "PayPal":
                    ordNop.PaymentMethodSystemName = "Payments.PayPalStandard";
                    break;
                case "VisaMC":
                case "AmEx":
                    ordNop.PaymentMethodSystemName = "Payments.PayPalDirect";
                    break;
                default:
                    ordNop.PaymentMethodSystemName = ordEBay.PaymentMethod;
                    break;
            }
            ordNop.OrderTotal = ordEBay.OrderTotal;
            ordNop.OrderSubtotalExclTax = ordNop.OrderSubtotalInclTax = (ordNop.OrderTotal - ordNop.OrderShippingExclTax);

            ordNop.ShippingMethod = ordEBay.ShippingMethod;
            ordNop.OrderShippingExclTax = ordNop.OrderShippingInclTax = ordEBay.ShippingCost;


            // Add the order items...
            foreach (eBayTransaction trans in ordEBay.Transactions)
            {
                // Try to find SKU from eBay...
                string strSku = trans.ItemSku; //...attempt to get product SKU from eBay
                if (String.IsNullOrEmpty(strSku))
                    strSku = trans.ItemTitle.Split('{', '}')?[1].Trim(); //...eBay didn't have one, Roadless Gear embeds SKU's in the title using braces - try to extract from there

                OrderItem orderItem = new OrderItem
                {
                    UnitPriceInclTax = trans.UnitPrice,
                    Quantity = trans.Quantity
                };

                if (String.IsNullOrEmpty(strSku))
                    orderItem.ProductId = _settings.eBayDefaultProductId; //...could not find any SKU on eBay, use default product
                else
                    orderItem.ProductId = _productService.GetProductBySku(strSku)?.Id ?? _settings.eBayDefaultProductId; //...assign ProductId (or default product if SKU lookup fails)
                ordNop.OrderItems.Add(orderItem);
            }

            // Save order...
            _orderService.InsertOrder(ordNop);
            // Save additional eBay information in GenericAttribute table...
            _genericAttributeService.SaveAttribute(ordNop, "eBayOrderId", ordEBay.OrderId, _settings.eBayDefaultStoreId);
            _genericAttributeService.SaveAttribute(ordNop, "eBayMessage", ordEBay.BuyerMessage, _settings.eBayDefaultStoreId);
            _genericAttributeService.SaveAttribute(ordNop, "eBayUserName", ordEBay.BuyerUserId, _settings.eBayDefaultStoreId);

            return ordNop;
        }

        /// <summary>
        /// Lookup eBay customer info if email matches existing nopCommerce customer, use that.  Otherwise add new guest customer to nopCommerce
        /// NOTE: For addresses, since eBay only has Shipping address, we will leave the Billing address alone.  
        ///       Means existing customers have their Billing address untouched, and new customers end up with a null Billing address
        ///       TODO: Is a null Billing address going to cause some other function of nopCommerce to puke and die???
        /// </summary>
        /// <param name="ordEBay">eBay Order object</param>
        /// <returns>nopCommerce Customer</returns>
        private Customer eBayCustomerAddOrUpdate(eBayOrder ordEBay)
        {
            // If buyer's email exists in nopCommerce use that customer record, else add new guest customer...
            Customer cust = _customerService.GetCustomerByEmail(ordEBay.BuyerEmail) ?? _customerService.InsertGuestCustomer();
            if (cust == null)
            {
                _logger.Error("eBayCommander Error: Could neither find existing customer nor InsertGuestCustomer()!");
                return null;
            }
            cust.Email = ordEBay.BuyerEmail;
            if (cust.ShippingAddress == null)
            {
                // No shipping address (e.g. new customer)  Create one now...
                Address address = new Address { CreatedOnUtc = DateTime.UtcNow };
                cust.Addresses.Add(address);
                cust.ShippingAddress = address;
            }
            // NOTE: it doesn't matter if this is a new or an existing customer, the shipping address will always be set (or overwritten) to the current eBay address
            FillNopAddressFromEBay(ordEBay, cust.ShippingAddress);
            _customerService.UpdateCustomer(cust);
            return cust;
        }

        /// <summary>
        /// Does this eBay order already exist in nopCommerce?
        /// </summary>
        /// <param name="eBayOrderId">eBay Order Id to check</param>
        /// <returns>true if order exists, false otherwise</returns>
        private bool eBayOrderAlreadyProcessed(string eBayOrderId)
        {
            // We store the eBayOrder ID in the GenericAttribute table.   nopCommerce already has several helper functions to query data from this table but they do not have
            // any method of querying for a specific value.   So we just query the repository directly....
            string keyGroup = "Order";
            string key = "eBayOrderId";
            string value = eBayOrderId;
            var query = from ga in _genericAttributeRepository.Table
                        orderby ga.Id
                        where ga.KeyGroup == keyGroup && ga.Key == key && ga.Value == value
                        select ga;
            return (query.FirstOrDefault() != null);   
        }

        /// <summary>
        /// Given a nopCommerce Address object, fills with Shipping info from eBay order.
        /// NOTE: eBay doesn't support the concept of Billing vs. Shipping address.   There is only a Shipping address.
        /// </summary>
        /// <param name="ordEBay">eBay Order to read address info from</param>
        /// <param name="address">nopCommerce address to copy address info to</param>
        private void FillNopAddressFromEBay(eBayOrder ordEBay, Address address)
        {
            address.Email = ordEBay.BuyerEmail;
            address.PhoneNumber = ordEBay.Phone;
            address.FirstName = ordEBay.BuyerFirstName;
            address.LastName = ordEBay.BuyerLastName;
            address.Company = ordEBay.ShippingAddress1;
            address.Address1 = ordEBay.ShippingAddress2;
            address.Address2 = ordEBay.ShippingAddress3;
            address.City = ordEBay.ShippingCity;
            address.ZipPostalCode = ordEBay.ShippingZip;

            // eBay enjoys various ways to express itself (much to our joy)....
            switch (ordEBay.ShippingCountry.Trim().ToUpper())
            {
                case "US":
                case "USA":
                case "UNITED STATES":
                    // United States address...
                    address.Country = _countryService.GetCountryByTwoLetterIsoCode("US");
                    address.CountryId = address.Country.Id;
                    address.StateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(ordEBay.ShippingState);
                    if (address.StateProvince != null)
                        address.StateProvinceId = address.StateProvince.Id;
                    else
                        address.ZipPostalCode = $"{ordEBay.ShippingState} {address.ZipPostalCode}"; //...if state lookup fails, put text in front of zip
                    break;

                case "PUERTO RICO":
                    // eBay (and many Puerto Ricans) consider themselves to be a separate country.  Be that as it may, the US Postal Service considers them a US State and that's what matters to us...
                    address.Country = _countryService.GetCountryByTwoLetterIsoCode("US"); //...country will always be "US"
                    address.CountryId = address.Country.Id;
                    address.StateProvince = _stateProvinceService.GetStateProvinceByAbbreviation("PR"); //...state will always be "PR"
                    break;

                default:
                    // Look up country name specified in eBay...
                    var qc = from c in _countryRepository.Table
                        orderby c.Id
                        where c.Name == ordEBay.ShippingCountry
                        select c;
                    address.Country = qc.FirstOrDefault();
                    if (address.Country != null)
                        address.CountryId = address.Country.Id;
                    else
                        address.CountryId = null;

                    // Look up state/province name specified in eBay...
                    var qs = from s in _stateProvinceRepository.Table
                        orderby s.Id
                        where s.Name == ordEBay.ShippingState
                        select s;
                    address.StateProvince = qs.FirstOrDefault();
                    if (address.StateProvince != null)
                        address.StateProvinceId = address.StateProvince.Id;
                    else
                        address.ZipPostalCode = $"{ordEBay.ShippingState} {address.ZipPostalCode}"; //...if state lookup fails, put text in front of zip

                    break;
            }

        }
    }
}
