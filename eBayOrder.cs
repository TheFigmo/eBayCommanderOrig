using System;
using System.Collections.Generic;
using System.Globalization;

using eBay.Service.Util;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;

namespace RG.Plugin.eBayCommander
{
    public class eBayTransaction
    {
        public string ItemId { get; set; }
        public string ItemSku { get; set; }
        public string ItemTitle { get; set; }
        public string VariationSku { get; set; }
        public string OrderLineItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice  { get; set; }
        public string AttributeDescription  { get; set; }

        public eBayTransaction()
        {
        }
    }

    public class eBayOrder
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int OrderStatusId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
        public string BuyerName { get; set; }
        public string BuyerUserId { get; set; }
        public string BuyerEmail { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingAddress3 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCountry { get; set; }
        public string Phone { get; set; }
        public string EMail { get; set; }
        public decimal ShippingWeight { get; set; }
        public decimal ShippingCost { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingTrackingNumber { get; set; }
        public string ShippingCarrier { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal OrderTotal { get; set; }
        public string BuyerMessage { get; set; }
        public List<eBayTransaction> Transactions { get; set; } = new List<eBayTransaction>();
        public string Notes { get; set; }
        public string BuyerFirstName
        {
            get
            {
                if (BuyerName == null)
                    return null;
                string retval = BuyerName;
                if (retval.Contains(" "))
                    retval = retval.Substring(0, retval.IndexOf(" ")).Trim();
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(retval);
            }
        }
        public string BuyerLastName
        {
            get
            {
                if (BuyerName == null)
                    return null;
                string retval = "";
                if (BuyerName.Contains(" "))
                    retval = BuyerName.Substring(BuyerName.IndexOf(" ")+1).Trim();
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(retval);
            }
        }

        static public bool IsInitialized { get { return m_eBayApiContext != null; } }

        public static ApiContext m_eBayApiContext = null;

        public eBayOrder()
        {
        }

        public static void Init(string eBayToken)
        {
            // -----------------------------------------------------------------
            // Init eBay API...
            m_eBayApiContext = new ApiContext();
            m_eBayApiContext.ApiCredential.eBayToken = eBayToken; //  AgAAAA**AQAAAA**aAAAAA**loOuVQ**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wGl4CnC5mEoAidj6x9nY+seQ**XxoCAA**AAMAAA**taogDK5blMcwXDqOKE9FAZ2Kyl9TPWxDdGBDw7iHiCEOFpz+kemua2dQDQBEbqe4oLhgNtJA/tinF6kgjn/YPoFkVIwVeuPUL0TjE/WiJmLGSstDWYzJ7ctVMb68B1qmXmq57AKeoqypnib0QUs9SGhgMmcU2tpg1pe6YllqgCGf4Jci1y9F2jfmSG+Q/0puA6AHje6drTe8U4EzyzPOh2yxfNRrKd2cwmhiHM8gXMYTruCx1vVbQ1sRTgyx7deCECVCBR9R4ULsGid3wpi1kUCX7rFT0/79vuf/y7AhkqSi7cVRStuEyg9xiC/tAsTBiXBUhaPsbYa1Fq1B/h0ddYcn93NPFSWOypJWpS59/NDTBEV61T9lyJvDpbqpwI7DhaF2pX6zh2UpKPbGO97l3AzphOeMttvnJWP8i4izlubRI4NNPztV52MX3jAun0Eca3MEVuJflMy4pO6BV1/XFnZ+920uGKkj44Sojt4jiZqzJpPapWbhNYHWod7J4Dshq1ekHxl5lzSeUY4OD729CaGNUMBBUvXPKAo50JxZ6f4/3jbPs6DLZwp+kTvqLBr+N+fTk5NsQQTNeAQcO21KnChEOqr8iTKVfoCv/fqBxwvUkYrNx6dxjOoE4r96iXHEGR9yLojwvOHCESTw3UsW/ZWD9rMvvdZK285VjngS9W9XwzJQ/vTI9H01YAfsRT1Ie15d6khLgbGRLIrGpld139ZYU2KrW5R+2Xas/WyvGEHdkwbQ8xhvIgP8b1Lwsa0p
            m_eBayApiContext.SoapApiServerUrl = "https://api.ebay.com/wsapi";
            m_eBayApiContext.Version = "855";
            m_eBayApiContext.Site = eBay.Service.Core.Soap.SiteCodeType.US;
            //enable logging
            //        Utility.GLOBALS.m_eBayApiContext.ApiLogManager = new eBay.Service.Core.Sdk.ApiLogManager();
            //        Utility.GLOBALS.m_eBayApiContext.ApiLogManager.ApiLoggerList.Add(new eBay.Service.Util.FileLogger("log.txt", true, true, true));
            //        Utility.GLOBALS.m_eBayApiContext.ApiLogManager.EnableLogging = true;
            // -------------------------------------------------------------------
        }

        public static string FetchToken()
        {
            string strRetVal = null;
            FetchTokenCall fetchToken = new FetchTokenCall(m_eBayApiContext);
            fetchToken.Execute();
            if (fetchToken.ApiResponse.Ack != AckCodeType.Failure)
                strRetVal = fetchToken.eBayToken;

            return strRetVal;
        }

        public static eBayOrder GetOrderById(string strId)
        {
            if (String.IsNullOrEmpty(strId))
                return null;

            try
            {
                GetOrdersCall getOrders = new GetOrdersCall(m_eBayApiContext);
                getOrders.DetailLevelList = new DetailLevelCodeTypeCollection();
                getOrders.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);

                getOrders.OrderIDList = new StringCollection();
                getOrders.OrderIDList.Add(strId);
                getOrders.Execute();

                if (getOrders.ApiResponse.Ack != AckCodeType.Failure)
                {
                    if (getOrders.ApiResponse.OrderArray.Count != 1)
                        return null; //...better be one and ONLY ONE order returned!!

                    eBayOrder retval = new eBayOrder
                    {
                        OrderId = getOrders.ApiResponse.OrderArray[0].OrderID,
                        OrderStatusId = Convert.ToInt16(getOrders.ApiResponse.OrderArray[0].OrderStatus),
                        OrderDate = TimeZoneInfo.ConvertTimeFromUtc(getOrders.ApiResponse.OrderArray[0].CreatedTime, TimeZoneInfo.Local),
                        PaymentMethod = getOrders.ApiResponse.OrderArray[0].CheckoutStatus.PaymentMethod.ToString(),
                        BuyerUserId = getOrders.ApiResponse.OrderArray[0].BuyerUserID
                    };
                    if (getOrders.ApiResponse.OrderArray[0].CheckoutStatus.Status == CompleteStatusCodeType.Complete)
                    {
                        retval.AdjustmentAmount = Convert.ToDecimal(getOrders.ApiResponse.OrderArray[0].AdjustmentAmount.Value);
                        retval.OrderTotal = Convert.ToDecimal(getOrders.ApiResponse.OrderArray[0].Total.Value);
                        retval.ShippingMethod = getOrders.ApiResponse.OrderArray[0].ShippingServiceSelected.ShippingService;
                        retval.ShippingCost = Convert.ToDecimal(getOrders.ApiResponse.OrderArray[0].ShippingServiceSelected.ShippingServiceCost.Value);
                        retval.BuyerName = getOrders.ApiResponse.OrderArray[0].ShippingAddress.Name;
                        retval.ShippingAddress1 = getOrders.ApiResponse.OrderArray[0].ShippingAddress.Street;
                        retval.ShippingAddress2 = getOrders.ApiResponse.OrderArray[0].ShippingAddress.Street1;
                        retval.ShippingAddress3 = getOrders.ApiResponse.OrderArray[0].ShippingAddress.Street2;
                        retval.ShippingCity = getOrders.ApiResponse.OrderArray[0].ShippingAddress.CityName;
                        retval.ShippingState = getOrders.ApiResponse.OrderArray[0].ShippingAddress.StateOrProvince;
                        retval.ShippingZip = getOrders.ApiResponse.OrderArray[0].ShippingAddress.PostalCode;
                        retval.ShippingCountry = getOrders.ApiResponse.OrderArray[0].ShippingAddress.CountryName;
                        retval.Phone = getOrders.ApiResponse.OrderArray[0].ShippingAddress.Phone;
                        retval.BuyerMessage = getOrders.ApiResponse.OrderArray[0].BuyerCheckoutMessage;
                        retval.PaymentStatus = getOrders.ApiResponse.OrderArray[0].CheckoutStatus.Status.ToString();
                    }


                    //Order could be comprised of one or more items
                    TransactionTypeCollection orderTrans = getOrders.ApiResponse.OrderArray[0].TransactionArray;
                    foreach (TransactionType transaction in orderTrans)
                    {
                        retval.BuyerEmail = transaction.Buyer.Email; //...wondering why the buyer email is on each transaction line but the buyer UserId is on the main order?
                        eBayTransaction trans = new eBayTransaction();
                        trans.ItemId = transaction.Item.ItemID;
                        trans.ItemSku = transaction.Item.SKU;
                        trans.ItemTitle = transaction.Item.Title;
                        trans.OrderLineItemId = transaction.OrderLineItemID;
                        trans.Quantity = transaction.QuantityPurchased;
                        trans.UnitPrice = Convert.ToDecimal(transaction.TransactionPrice.Value);

                        if (transaction.ShippingDetails.ShipmentTrackingDetails.Count > 0)
                        {
                            retval.ShippingTrackingNumber = transaction.ShippingDetails.ShipmentTrackingDetails[0].ShipmentTrackingNumber;
                            retval.ShippingCarrier = transaction.ShippingDetails.ShipmentTrackingDetails[0].ShippingCarrierUsed;
                        }


                        //If you are listing variation items, you will need to retrieve the variation details as chosen by the buyer
                        if (transaction.Variation != null)
                            if (transaction.Variation.SKU != null)
                                trans.VariationSku = transaction.Variation.SKU;

                        retval.Transactions.Add(trans);
                    }
                    return retval;
                }

            }
            catch (Exception) { /* DO NOTHING */ }

            return null;
        }

        public static List<eBayOrder> GetOrdersByDate(DateTime createTimeFrom, DateTime createTimeTo)
        {
            List<eBayOrder> FullList = new List<eBayOrder>();

            GetOrdersCall getOrders = new GetOrdersCall(m_eBayApiContext)
            {
                DetailLevelList = new DetailLevelCodeTypeCollection {DetailLevelCodeType.ReturnAll},
// *REMOVED BY FIGMO*  used to deal in whole days only - Always from midnight on starting date to 1 minute before midnight on ending date (UTC time)
//                CreateTimeFrom = TimeZoneInfo.ConvertTimeToUtc(CreateTimeFrom.Date.Add(new TimeSpan(00, 00, 00)), TimeZoneInfo.Local),
//                CreateTimeTo = TimeZoneInfo.ConvertTimeToUtc(CreateTimeTo.Date.Add(new TimeSpan(23, 59, 59)), TimeZoneInfo.Local),
                CreateTimeFrom = createTimeFrom,
                CreateTimeTo = createTimeTo,
                Pagination = new PaginationType
                {
                    EntriesPerPage = 100,
                    PageNumber = 1
                }
            };

            getOrders.Execute();
            if (getOrders.ApiResponse.Ack != AckCodeType.Failure)
            {
                while (getOrders.ApiResponse.OrderArray.Count != 0)
                {
                    // Add all orders in response to return value (FullList)...
                    foreach (OrderType order in getOrders.ApiResponse.OrderArray)
                    {
                        eBayOrder ord = new eBayOrder
                        {
                            OrderId = order.OrderID,
                            OrderStatusId = Convert.ToInt16(order.OrderStatus),
                            OrderDate = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedTime, TimeZoneInfo.Local),
                            PaymentMethod = order.CheckoutStatus.PaymentMethod.ToString(),
                            BuyerUserId = order.BuyerUserID,
                            OrderStatus = order.OrderStatus.ToString()
                        };
                        if (order.CheckoutStatus.Status == CompleteStatusCodeType.Complete)
                        {
                            ord.AdjustmentAmount = Convert.ToDecimal(order.AdjustmentAmount.Value);
                            ord.OrderTotal = Convert.ToDecimal(order.Total.Value);
                            ord.ShippingMethod = order.ShippingServiceSelected.ShippingService;
                            ord.ShippingCost = Convert.ToDecimal(order.ShippingServiceSelected.ShippingServiceCost.Value);
                            ord.BuyerName = order.ShippingAddress.Name;
                            ord.ShippingAddress1 = order.ShippingAddress.Street;
                            ord.ShippingAddress2 = order.ShippingAddress.Street1;
                            ord.ShippingAddress3 = order.ShippingAddress.Street2;
                            ord.ShippingCity = order.ShippingAddress.CityName;
                            ord.ShippingState = order.ShippingAddress.StateOrProvince;
                            ord.ShippingZip = order.ShippingAddress.PostalCode;
                            ord.ShippingCountry = order.ShippingAddress.CountryName;
                            ord.Phone = order.ShippingAddress.Phone;
                            ord.BuyerMessage = order.BuyerCheckoutMessage;
                            ord.PaymentStatus = order.CheckoutStatus.Status.ToString();
                        }

                        //Order could be comprised of one or more items
                        TransactionTypeCollection orderTrans = order.TransactionArray;
                        foreach (TransactionType transaction in orderTrans)
                        {
                            ord.BuyerEmail = transaction.Buyer.Email; //...wondering why the buyer email is on each transaction line but the buyer UserId is on the main order?
                            eBayTransaction trans = new eBayTransaction();
                            trans.ItemId = transaction.Item.ItemID;
                            trans.ItemSku = transaction.Item.SKU;
                            trans.ItemTitle = transaction.Item.Title;
                            trans.OrderLineItemId = transaction.OrderLineItemID;
                            trans.Quantity = transaction.QuantityPurchased;
                            trans.UnitPrice = Convert.ToDecimal(transaction.TransactionPrice.Value);

                            if (transaction.ShippingDetails.ShipmentTrackingDetails.Count > 0)
                            {
                                ord.ShippingTrackingNumber = transaction.ShippingDetails.ShipmentTrackingDetails[0].ShipmentTrackingNumber;
                                ord.ShippingCarrier = transaction.ShippingDetails.ShipmentTrackingDetails[0].ShippingCarrierUsed;
                            }

                            //If you are listing variation items, you will need to retrieve the variation details as chosen by the buyer
                            if (transaction.Variation != null)
                                if (transaction.Variation.SKU != null)
                                    trans.VariationSku = transaction.Variation.SKU;

                            ord.Transactions.Add(trans);
                        }

                        FullList.Add(ord);
                    }

                    // There is a max 100 orders per call so here we check for more data and increment page number and repeat if needed...
                    if (!getOrders.HasMoreOrders)
                        break; //...no more data
                    getOrders.Pagination.PageNumber++;
                    getOrders.Execute();
                }
            }

            return FullList;
        }

        public static void MarkAsShipped(string strId, string strTracking)
        {
            strTracking = strTracking.Trim().ToUpper();
            eBayOrder order = GetOrderById(strId);
            CompleteSaleCall completeSale = new CompleteSaleCall(m_eBayApiContext);
            completeSale.OrderLineItemID = order.Transactions[0].OrderLineItemId;
            completeSale.Shipment = new ShipmentType();
            // Rules for ID'ing carrier based on tracking number:   
            //      1. If it starts with "1Z" it's UPS (easy one)
            //      2. If it ends in "US" it's a USPS international tracking number
            //      3. If it is over 20 chars in length it's USPS (they have 22 char long tracking numbers with no ID code anywhere in them)
            //      4. If it is none of the above it's FedEx (rules I've read on the web claim numbers are all 12-14 chars)
            if (strTracking.Contains("1Z"))
                completeSale.Shipment.ShippingCarrierUsed = "UPS";  //...UPS (easy one)
            else if (strTracking.Substring(strTracking.Length - 2) == "US")
                completeSale.Shipment.ShippingCarrierUsed = "USPS"; //...USPS international tracking #'s all end in "US" (e.g. 'LZ962114784US' or 'CV039854566US')
            else if (strTracking.Length > 20)
                completeSale.Shipment.ShippingCarrierUsed = "USPS"; //...USPS domestic tracking #'s are quite long (e.g. '9405803699300213427328' or '9405803699300211393212')
            else
                completeSale.Shipment.ShippingCarrierUsed = "FedEx";    //...if it's neither UPS nor USPS - then it must be FedEx
            completeSale.Shipment.ShipmentTrackingNumber = strTracking;
            completeSale.Execute();
        }
    }
}
