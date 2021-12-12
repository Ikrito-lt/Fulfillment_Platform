using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Modules {
    class OrderModule {
        private readonly string getOrdersEndPoint = "https://real-europe-corp.myshopify.com/admin/api/2021-07/orders.json";
        //
        // getting orders from Shopyfi API
        //

        //method for getting orders from Shopyfi API
        public List<Order> getNewOrders() {

            var client = new RestClient(getOrdersEndPoint);
            var request = new RestRequest();
            request.AddHeader("Authorization", Globals.getBase64ShopifyCreds());

            var response = client.Get(request);

            if (response.IsSuccessful) {
                string content = response.Content; // Raw content as string

                content = content[content.IndexOf("[")..];
                content = content.Remove(content.Length - 1, 1);

                List<Order> orderList = JsonConvert.DeserializeObject<List<Order>>(content);

                return foo(orderList);
            } else {
                throw new Exception($"Getting orders from Shopify API yelded in Exception:\n" +
                    $"Status message: {response.StatusCode}\n" +
                    $"Responce content: {response.Content}");
            }
        }

        public List<Order> foo(List<Order> orders) {
            List<Order> bar = orders;

            DataBaseInterface db = new();
            var DBOrders = db.Table("Orders").Get("ID, Shopify_ID");
            if (DBOrders.Count > 0) {
                foreach (var order in DBOrders) {
                    var orderKVP = order.Value;
                    bar.RemoveAll(x => x.id == orderKVP["Shopify_ID"]);
                }
            };

            return bar;
        }


        //
        //
        //
        //method for getting orders form DB
        public List<Order> getFulfilledOrders() {
            List<Order> FulfilledOrders = new();
            DataBaseInterface db = new();

            var orders = db.Table("Orders").Get();
            foreach (var order in orders.Values) {
                Order newOrder = new();

                //shopify_ID
                newOrder.id = order["Shopify_ID"];
                //DB_ID
                newOrder.DBID = order["ID"];

                newOrder.contact_email = order["Contact_Email"];
                newOrder.confirmed = order["Confirmed"] == "1";

                //date stuff
                newOrder.created_at = order["Created_At"];
                newOrder.cancelled_at = order["Cancelled_At"];
                newOrder.closed_at = order["Closed_At"];

                //money stuff
                newOrder.currency = order["Currency"];
                newOrder.current_subtotal_price = double.Parse(order["Subtotal_Price"]);
                newOrder.current_total_discounts = double.Parse(order["Total_Discounts"]);
                newOrder.current_total_price = double.Parse(order["Total_Price"]);
                newOrder.current_total_tax = double.Parse(order["Total_Tax"]);
                newOrder.financial_status = order["Finantial_Status"];
                newOrder.name = order["Name"];
                newOrder.note = order["Note"];
                newOrder.phone = order["Phone"];

                //other stuff
                string billingAddressID = order["BillingAddress_ID"];
                string ShippingAddressID = order["ShippingAddress_ID"];
                string CustomerID = order["Customer_ID"];

                newOrder.customer = GetCustomer(CustomerID);
                newOrder.shipping_address = GetAddress(ShippingAddressID);
                newOrder.billing_address = GetAddress(billingAddressID);

                newOrder.line_items = GetOrderProducts(newOrder.DBID);

                //public List<OrderProduct> line_items { set; get; 

                FulfilledOrders.Add(newOrder);
            }
            return FulfilledOrders;
        }

        //method for getting address from database
        private List<OrderProduct> GetOrderProducts(string OrderID) {
            List<OrderProduct> orderProducts = new();

            DataBaseInterface db = new();
            var whereGet = new Dictionary<string, Dictionary<string, string>> {
                ["Order_ID"] = new Dictionary<string, string> {
                    ["="] = OrderID
                }
            };
            var orderProductsDB = db.Table($"OrderProducts").Where(whereGet).Get();

            foreach (var op in orderProductsDB.Values) {
                OrderProduct orderProduct = new();

                orderProduct.id = op["Shopify_ID"];
                orderProduct.OrderDBID = op["Order_ID"];
                orderProduct.DBID = op["ID"];
                orderProduct.sku = op["SKU"];
                orderProduct.name = op["Name"];
                orderProduct.vendor = op["Vendor"];
                orderProduct.quantity = int.Parse(op["Quantity"]);
                orderProduct.price = double.Parse(op["Price"]);
                orderProduct.total_discount = double.Parse(op["TotalDiscount"]);
                orderProduct.grams = int.Parse(op["Grams"]);
                orderProduct.product_exists = op["Taxable"] == "1";
                orderProduct.taxable = op["ProductExists"] == "1";

                orderProduct.product_id = op["Shopify_Product_ID"];
                orderProduct.variant_id = op["Shopify_Variant_ID"];

                orderProducts.Add(orderProduct);
            }

            return orderProducts;
        }

        //method for getting address from database
        private Address GetAddress(string addressID) {
            Address address = new();

            DataBaseInterface db = new();
            var whereGet = new Dictionary<string, Dictionary<string, string>> {
                ["ID"] = new Dictionary<string, string> {
                    ["="] = addressID
                }
            };
            var addressDB = db.Table($"Addresses").Where(whereGet).Get()[0];

            //DB_ID
            address.DBID = addressID;

            address.first_name = addressDB["FirstName"];
            address.last_name = addressDB["LastName"];
            address.company  = addressDB["Company"];
            address.address1 = addressDB["Address1"];
            address.address2 = addressDB["Address2"];
            address.city  = addressDB["City"];
            address.province = addressDB["Province"];
            address.country  = addressDB["Country"];
            address.zip = addressDB["Zip"];
            address.country_code = addressDB["CountryCode"];
            address.province_code = addressDB["ProvinceCode"];
            address.name = addressDB["Name"];
            address.phone = addressDB["Phone"];
            address.latitude = addressDB["Latitude"];
            address.longditude  = addressDB["Longditude"];

            return address;
        }

        //method for getting customer from database
        private Customer GetCustomer(string customerID) {
            Customer customer = new();

            DataBaseInterface db = new();
            var whereGet = new Dictionary<string, Dictionary<string, string>> {
                ["ID"] = new Dictionary<string, string> {
                    ["="] = customerID
                }
            };
            var customerDB = db.Table($"Customers").Where(whereGet).Get()[0];

            //shopify_ID
            customer.id = customerDB["Shopify_ID"];
            //DB_ID
            customer.DBID = customerID;
            customer.email = customerDB["Email"];
            customer.phone = customerDB["Phone"];
            customer.first_name = customerDB["FirstName"];
            customer.last_name = customerDB["LastName"];
            customer.created_at = customerDB["Created_At"];
            customer.updated_at = customerDB["Updated_At"];
            customer.total_spent = double.Parse(customerDB["TotalSpent"]);
            customer.state = customerDB["State"];
            customer.orders_count = int.Parse(customerDB["OrdersCount"]);
            customer.last_order_id = customerDB["LastOrder_ID"];
            customer.verified_email = customerDB["VerifiedEmail"] == "1";
            customer.note = customerDB["Note"];

            return customer;
        }


        //
        // Fulfilling orders section
        //

        //method that fulfills order i.e. saves it to DB
        public void FulFillOrder(Order order) {
            DataBaseInterface db = new();
            var customers = db.Table("Customers").Get();
            List<Dictionary<string, string>> CustomerList = new();

            foreach (var c in customers) {
                CustomerList.Add(c.Value);
            }

            if (CustomerList.Exists(x => x["Shopify_ID"] == order.customer.id)) {
                string customerDBID = CustomerList.Find(x => x["Shopify_ID"] == order.customer.id)["ID"];
                InsertOrder(order, customerDBID);
            } else {
                InsertOrderNewCustomer(order);
            }
        }

        //inserting order data to DB but customer is already in DB
        public void InsertOrder(Order order, string customerDBID) {
            DataBaseInterface db = new();

            //inserting shipping address
            var shipping_address = order.shipping_address;
            var ShippingAddressInsertData = new Dictionary<string, string> {
                ["FirstName"] = shipping_address.first_name,
                ["LastName"] = shipping_address.last_name,
                ["Company"] = shipping_address.company,
                ["Address1"] = shipping_address.address1,
                ["Address2"] = shipping_address.address2,
                ["City"] = shipping_address.city,
                ["Province"] = shipping_address.province,
                ["Country"] = shipping_address.country,
                ["Zip"] = shipping_address.zip,
                ["CountryCode"] = shipping_address.country_code,
                ["ProvinceCode"] = shipping_address.province_code,
                ["Name"] = shipping_address.name,
                ["Phone"] = shipping_address.phone,
                ["Latitude"] = shipping_address.latitude,
                ["Longditude"] = shipping_address.longditude
            };
            string ShippingAddress_ID = db.Table("Addresses").Insert(ShippingAddressInsertData);


            //inserting Billing address 
            var billing_address = order.shipping_address;
            var BillingAddressInsertData = new Dictionary<string, string> {
                ["FirstName"] = billing_address.first_name,
                ["LastName"] = billing_address.last_name,
                ["Company"] = billing_address.company,
                ["Address1"] = billing_address.address1,
                ["Address2"] = billing_address.address2,
                ["City"] = billing_address.city,
                ["Province"] = billing_address.province,
                ["Country"] = billing_address.country,
                ["Zip"] = billing_address.zip,
                ["CountryCode"] = billing_address.country_code,
                ["ProvinceCode"] = billing_address.province_code,
                ["Name"] = billing_address.name,
                ["Phone"] = billing_address.phone,
                ["Latitude"] = billing_address.latitude,
                ["Longditude"] = billing_address.longditude
            };
            string BillingAddress_ID = db.Table("Addresses").Insert(BillingAddressInsertData);

            //insert order data 
            var OrderInsertData = new Dictionary<string, string> {
                ["Contact_Email"] = order.contact_email,
                ["Confirmed"] = order.confirmed.ToString(),
                ["Created_At"] = order.created_at,
                ["Cancelled_At"] = order.cancelled_at,
                ["Closed_At"] = order.closed_at,
                ["Currency"] = order.currency,
                ["Subtotal_Price"] = order.current_subtotal_price.ToString(),
                ["Total_Discounts"] = order.current_total_discounts.ToString(),
                ["Total_Price"] = order.current_total_price.ToString(),
                ["Total_Tax"] = order.current_total_tax.ToString(),
                ["Finantial_Status"] = order.financial_status,
                ["Name"] = order.name,
                ["Note"] = order.note,
                ["Phone"] = order.contact_email,
                ["BillingAddress_ID"] = BillingAddress_ID,
                ["ShippingAddress_ID"] = ShippingAddress_ID,
                ["Customer_ID"] = customerDBID,
                ["Shopify_ID"] = order.id,
            };
            string OrderID = db.Table("Orders").Insert(OrderInsertData);

            //updating customers last order id
            var updateData = new Dictionary<string, string> {
                ["LastOrder_ID"] = OrderID

            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["ID"] = new Dictionary<string, string> {
                    ["="] = customerDBID
                }
            };
            db.Table($"Customers").Where(whereUpdate).Update(updateData);

            //insert order products
            foreach (OrderProduct p in order.line_items) {
                var OrderProductInsertData = new Dictionary<string, string> {
                    ["Order_ID"] = OrderID,
                    ["SKU"] = p.sku,
                    ["Name"] = p.name,
                    ["Vendor"] = p.vendor,
                    ["Quantity"] = p.quantity.ToString(),
                    ["Price"] = p.price.ToString(),
                    ["TotalDiscount"] = p.total_discount.ToString(),
                    ["Grams"] = p.grams.ToString(),
                    ["Taxable"] = p.taxable.ToString(),
                    ["ProductExists"] = p.product_exists.ToString(),
                    ["Shopify_ID"] = p.id,
                    ["Shopify_Product_ID"] = p.product_id,
                    ["Shopify_Variant_ID"] = p.variant_id
                };
                db.Table("OrderProducts").Insert(OrderProductInsertData);
            }

        }

        //inserting order data to DB but customer is not in DB
        public void InsertOrderNewCustomer(Order order) {
            DataBaseInterface db = new();

            //inserting shipping address
            var shipping_address = order.shipping_address;
            var ShippingAddressInsertData = new Dictionary<string, string> {
                ["FirstName"] = shipping_address.first_name,
                ["LastName"] = shipping_address.last_name,
                ["Company"] = shipping_address.company,
                ["Address1"] = shipping_address.address1,
                ["Address2"] = shipping_address.address2,
                ["City"] = shipping_address.city,
                ["Province"] = shipping_address.province,
                ["Country"] = shipping_address.country,
                ["Zip"] = shipping_address.zip,
                ["CountryCode"] = shipping_address.country_code,
                ["ProvinceCode"] = shipping_address.province_code,
                ["Name"] = shipping_address.name,
                ["Phone"] = shipping_address.phone,
                ["Latitude"] = shipping_address.latitude,
                ["Longditude"] = shipping_address.longditude
            };
            string ShippingAddress_ID = db.Table("Addresses").Insert(ShippingAddressInsertData);


            //inserting Billing address 
            var billing_address = order.shipping_address;
            var BillingAddressInsertData = new Dictionary<string, string> {
                ["FirstName"] = billing_address.first_name,
                ["LastName"] = billing_address.last_name,
                ["Company"] = billing_address.company,
                ["Address1"] = billing_address.address1,
                ["Address2"] = billing_address.address2,
                ["City"] = billing_address.city,
                ["Province"] = billing_address.province,
                ["Country"] = billing_address.country,
                ["Zip"] = billing_address.zip,
                ["CountryCode"] = billing_address.country_code,
                ["ProvinceCode"] = billing_address.province_code,
                ["Name"] = billing_address.name,
                ["Phone"] = billing_address.phone,
                ["Latitude"] = billing_address.latitude,
                ["Longditude"] = billing_address.longditude
            };
            string BillingAddress_ID = db.Table("Addresses").Insert(BillingAddressInsertData);

            //inserting Customer data 
            var customer = order.customer;
            var CustomerInsertData = new Dictionary<string, string> {
                ["Email"] = customer.email,
                ["Phone"] = customer.phone,
                ["FirstName"] = customer.first_name,
                ["LastName"] = customer.last_name,
                ["Created_At"] = customer.created_at,
                ["Updated_At"] = customer.updated_at,
                ["TotalSpent"] = customer.total_spent.ToString(),
                ["State"] = customer.state,
                ["OrdersCount"] = customer.orders_count.ToString(),
                ["VerifiedEmail"] = customer.verified_email.ToString(),
                ["Note"] = customer.note,
                ["Shopify_ID"] = customer.id
            };
            string Custommer_ID = db.Table("Customers").Insert(CustomerInsertData);

            //insert order data 
            var OrderInsertData = new Dictionary<string, string> {
                ["Contact_Email"] = order.contact_email,
                ["Confirmed"] = order.confirmed.ToString(),
                ["Created_At"] = order.created_at,
                ["Cancelled_At"] = order.cancelled_at,
                ["Closed_At"] = order.closed_at,
                ["Currency"] = order.currency,
                ["Subtotal_Price"] = order.current_subtotal_price.ToString(),
                ["Total_Discounts"] = order.current_total_discounts.ToString(),
                ["Total_Price"] = order.current_total_price.ToString(),
                ["Total_Tax"] = order.current_total_tax.ToString(),
                ["Finantial_Status"] = order.financial_status,
                ["Name"] = order.name,
                ["Note"] = order.note,
                ["Phone"] = order.contact_email,
                ["BillingAddress_ID"] = BillingAddress_ID,
                ["ShippingAddress_ID"] = ShippingAddress_ID,
                ["Customer_ID"] = Custommer_ID,
                ["Shopify_ID"] = order.id,
            };
            string OrderID = db.Table("Orders").Insert(OrderInsertData);

            //updating customers last order id
            var updateData = new Dictionary<string, string> {
                ["LastOrder_ID"] = OrderID

            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["ID"] = new Dictionary<string, string> {
                    ["="] = Custommer_ID
                }
            };
            db.Table($"Customers").Where(whereUpdate).Update(updateData);

            //insert order products
            foreach (OrderProduct p in order.line_items) {
                var OrderProductInsertData = new Dictionary<string, string> {
                    ["Order_ID"] = OrderID,
                    ["SKU"] = p.sku,
                    ["Name"] = p.name,
                    ["Vendor"] = p.vendor,
                    ["Quantity"] = p.quantity.ToString(),
                    ["Price"] = p.price.ToString(),
                    ["TotalDiscount"] = p.total_discount.ToString(),
                    ["Grams"] = p.grams.ToString(),
                    ["Taxable"] = p.taxable.ToString(),
                    ["ProductExists"] = p.product_exists.ToString(),
                    ["Shopify_ID"] = p.id,
                    ["Shopify_Product_ID"] = p.product_id,
                    ["Shopify_Variant_ID"] = p.variant_id
                };
                db.Table("OrderProducts").Insert(OrderProductInsertData);
            }

        }
    }
}
