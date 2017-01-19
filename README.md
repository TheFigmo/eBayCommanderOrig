# eBayCommander v1.0
Import orders from eBay into nopCommerce 3.70

This nopCommerce plugin automatically imports orders from your eBay store to your nopCommerce store.   Currently it works with version 3.70.  May work on other versions.   How can you tell?  Install it and try it.  (then let me know and I'll update this file)

# WHAT ELSE DO YOU NEED?
You'll need an API Token Key from eBay for this to work.  This Key is what authorizes our software to access your eBay account.   You can obtain this from here: https://go.developer.ebay.com   

# HOW TO MAKE IT WORK?
Once you have your Token Key from eBay, install the plugin by copying the project to a folder under your plugins folder in your nopCommerce source code folder.   Than rebuild nopCommerce and publish it to your web server.  Be sure to restart the application.  Go to ADMIN->CONFIGURATION->PLUGINS->LOCAL PLUGINS and install the new eBayCommander plugin.  Once installed, check out the CONFIGURE page.

- eBay API Application Key: This is that Token Key we were talking about above.  Simply copy and paste it here.  The "Request API Token From eBay" button will open a new window and take you directly to the above link if you haven't gotten your Key yet.

- Default Store Id:  This is the nopCommerce StoreId of the store you want to add new orders to.   eBayCommander only supports importing eBay orders into a single store.  In my case, I created a "pseudo store" in nopCommerce called "eBay" and add all my eBay orders there (it shares all the same products with my main store).  Just so I can track which orders came from eBay and which from nopCommerce.

- Default Product Id:  eBayCommander matches eBay products on the order to the products in your nopCommerce store using the SKU.   For this to work, you'll have to specify a SKU on both your nopCommerce products, as well as your eBay listings.   If eBayCommander can't find a matching SKU for whatever reason, it will assign this Default Product to that order.  I reccomend you create a new product in nopCommerce, called "MISC" or "DEFAULT" or "UNKNOWN" or something.   Flag it as not visible so it won't show up in your store to customers.   Than enter the Id of this product here.    This eliminates the need to be checking some error log all the time to find eBay orders that failed to import due to mismatched SKU numbers.   The order will be there in your list, but the product will be the "UNKNOWN" product so you'll know you'll have to fix the SKUs on that product (and/or the eBay listing).

- The "SAVE" button saves your settings.

- The "Check for new eBay orders now" button forces an eBay check now.   Normally this is a scheduled task that runs every "x" minutes.  But you can force a check by either clicking this button on this page, or by cliking the "RUN NOW" button on the SCHEDULED TASKS lists  (ADMIN->SYSTEM->SCHEDULED TASKS)

# BEHIND THE SCREENS

Some notes on how this plugin handles various bits of information from eBay:

* Each time eBayCommander checks for new orders - it looks back 24 hours from now.  This is done to for performance.   So you'll want to make sure you don't change the interval of the scheduled task to anything over a day.   And if you disable this plugin and come back a few days later and enable it again - be aware that it can only see new orders that were placed on eBay in the past day.

* Customer email's will be looked up, and if a match is found in nopCommerce that customer will be used for the order.  Otherwise, a new "Guest" customer will automatically be created.

* eBay doesn't have billing and shipping addresses.  Just shipping.   If using an existing customer, the customer's current billing address will be untouched.  And a new address will be added for the shipping address specified in eBay.    If this is a Guest customer, than the billing address will be NULL and shipping address will be the eBay shipping address.

* For the nopCommerce Order record - the shipping address specified in eBay will be used for BOTH the billing AND shipping addresses of the order.   nopCommerce doesn't allow Orders to have a null billing addresses

* The eBayOrderId and eBayUserName for each order are both preserved and attached to the new nopCommerce Order using the GenericAttribute table.   So you can modify your orders list to display this information or link back to the original eBay order.  You can either modify the Admin orders list to add this column or create your own page to display eBay orders.  This is optional.  You can just treat orders as orders regardless of where they originated.   Why would you want to customize your order list?  Knowing the eBayUserName is quite useful since if customers send you a message via eBay - eBay only gives your their user name.   It can be quite frustrating to get a message like  "FROM: SexyBeast1970:  What's the status of my order?" and then have to go lookup that user name to even know which customer is emailing you.    Also, your custom Order list can have a hyperlink that links you directly to the original eBay order page using the eBayOrderId stored in GenericAttribute.   This is useful if the SKU lookup failed and the new order only shows the "DEFAULT" nopProduct.   You can quickly jump to eBay and see exactly what it was they ordered (and update the SKU while you are at it)

* Each order added will publish the OrderPaidEvent() in nopCommerce so that any other plugins you may have installed that trigger on this event will recognize the new Orders.


