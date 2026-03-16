#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using R2V2.Core.CollectionManagement;
using R2V2.Infrastructure.GoogleAnalytics;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Models.Cart;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.OrderHistory;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Services
{
    public class GoogleService
    {
        private const int MaxBatchSize = 40;
        private readonly AnalyticsQueueService _analyticsQueueService;
        private readonly IClientSettings _clientSettings;
        private readonly ILog<GoogleService> _log;

        public GoogleService(
            ILog<GoogleService> log
            , IClientSettings clientSettings
            , AnalyticsQueueService analyticsQueueService
        )
        {
            _log = log;
            _clientSettings = clientSettings;
            _analyticsQueueService = analyticsQueueService;
        }

        public void LogProductClickAndDetail(InstitutionResource institutionResource, int index, string pageTitle)
        {
            if (institutionResource == null)
            {
                return;
            }

            var practiceArea = institutionResource.PracticeAreas?.FirstOrDefault();

            var googleProduct = new GoogleProduct
            {
                id = institutionResource.Isbn,
                name = institutionResource.Title,
                price = institutionResource.DiscountPrice,
                category = practiceArea?.Name,
                position = index,
                publisher = institutionResource.PublisherName
            };

            SendProductClick(googleProduct, pageTitle);
            SendProductDetail(googleProduct, pageTitle);
        }

        private void SendProductClick(GoogleProduct googleProduct, string pageTitle)
        {
            var postData = GetBasePostData("event");
            postData.Add("ec", "Ecommerce");
            postData.Add("ea", "Product Click");
            postData.Add("pal", pageTitle);
            var productData = GetProductDetail(googleProduct, "click", 1);
            foreach (var item in productData)
            {
                postData.Add(item.Key, item.Value);
            }

            SendData(postData);
        }

        private void SendProductDetail(GoogleProduct googleProduct, string pageTitle)
        {
            var postData = GetBasePostData("event");
            postData.Add("ec", "Ecommerce");
            postData.Add("ea", "Book Detail");
            postData.Add("pal", pageTitle);
            var productData = GetProductDetail(googleProduct, "detail", 1);
            foreach (var item in productData)
            {
                postData.Add(item.Key, item.Value);
            }

            SendData(postData);
        }

        public void LogAddToCart(CollectionAdd collectionAdd, string pageTitle)
        {
            if (collectionAdd == null || collectionAdd.InstitutionResource == null)
            {
                return;
            }

            var practiceArea = collectionAdd.InstitutionResource.PracticeAreas?.FirstOrDefault();

            var googleProduct = new GoogleProduct
            {
                id = collectionAdd.InstitutionResource.Isbn,
                name = collectionAdd.InstitutionResource.Title,
                price = collectionAdd.InstitutionResource.DiscountPrice,
                category = practiceArea?.Name,
                position = 1,
                quantity = collectionAdd.NumberOfLicenses,
                publisher = collectionAdd.InstitutionResource.PublisherName
            };
            SendAddToCart(googleProduct, pageTitle);
        }

        public void LogAddToCart(IOrderItem orderItem, string pageTitle)
        {
            var resourceOrderItem = orderItem as IResourceOrderItem;
            var productOrderItem = orderItem as IProductOrderItem;
            var googleProduct = new GoogleProduct();

            if (resourceOrderItem != null)
            {
                if (resourceOrderItem.CoreResource == null)
                {
                    return;
                }

                var practiceArea = resourceOrderItem.CoreResource.PracticeAreas?.FirstOrDefault();
                googleProduct = new GoogleProduct
                {
                    id = resourceOrderItem.CoreResource.Isbn,
                    name = resourceOrderItem.CoreResource.Title,
                    publisher = resourceOrderItem.CoreResource.Publisher?.Name,
                    price = resourceOrderItem.DiscountPrice,
                    category = practiceArea?.Name,
                    position = 1,
                    quantity = resourceOrderItem.NumberOfLicenses
                };
            }

            if (productOrderItem != null && productOrderItem.Include)
            {
                if (productOrderItem.Product == null)
                {
                    return;
                }

                googleProduct = new GoogleProduct
                {
                    id = $"Product-{productOrderItem.Product.Id}",
                    name = productOrderItem.Product.Name,
                    price = productOrderItem.DiscountPrice,
                    position = 1,
                    quantity = productOrderItem.NumberOfLicenses > 0 ? productOrderItem.NumberOfLicenses : 1
                };
            }

            SendAddToCart(googleProduct, pageTitle);
        }

        private void SendAddToCart(GoogleProduct googleProduct, string pageTitle)
        {
            var postData = GetBasePostData("event");
            postData.Add("ec", "Ecommerce");
            postData.Add("ea", "Add to Cart");
            postData.Add("pal", pageTitle);
            var productData = GetProductDetail(googleProduct, "add", 1);
            foreach (var item in productData)
            {
                postData.Add(item.Key, item.Value);
            }

            SendData(postData);
        }

        public void LogRemoveFromCart(IOrderItem orderItem, string pageTitle)
        {
            var resourceOrderItem = orderItem as IResourceOrderItem;
            var googleProduct = new GoogleProduct();

            if (resourceOrderItem != null)
            {
                if (resourceOrderItem.CoreResource == null)
                {
                    return;
                }

                var practiceArea = resourceOrderItem.CoreResource.PracticeAreas?.FirstOrDefault();
                googleProduct = new GoogleProduct
                {
                    id = resourceOrderItem.CoreResource.Isbn,
                    name = resourceOrderItem.CoreResource.Title,
                    publisher = resourceOrderItem.CoreResource.Publisher?.Name,
                    price = resourceOrderItem.DiscountPrice,
                    category = practiceArea?.Name,
                    position = 1,
                    quantity = resourceOrderItem.NumberOfLicenses
                };
            }

            SendRemoveFromCart(googleProduct, pageTitle);
        }

        private void SendRemoveFromCart(GoogleProduct googleProduct, string pageTitle)
        {
            var postData = GetBasePostData("event");
            postData.Add("ec", "Ecommerce");
            postData.Add("ea", "Remove from Cart");
            postData.Add("pal", pageTitle);
            var productData = GetProductDetail(googleProduct, "remove", 1);
            foreach (var item in productData)
            {
                postData.Add(item.Key, item.Value);
            }

            SendData(postData);
        }

        public void LogImpressions(List<InstitutionResource> institutionResources, string pageTitle)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var institutionResource in institutionResources)
            {
                var practiceArea = institutionResource.PracticeAreas?.FirstOrDefault();
                counter++;
                var googleProduct = new GoogleProduct
                {
                    id = institutionResource.Isbn,
                    name = institutionResource.Title,
                    price = institutionResource.DiscountPrice,
                    category = practiceArea?.Name,
                    position = counter,
                    publisher = institutionResource.PublisherName
                };
                googleProducts.Add(googleProduct);
            }

            SendImpressions(googleProducts, pageTitle);
        }

        private static Dictionary<string, string> GetImpressionProductDetail(GoogleProduct googleProduct, int counter)
        {
            return new Dictionary<string, string>
            {
                { string.Format("il{1}pi{0}id", googleProduct.position, counter), googleProduct.id },
                { string.Format("il{1}pi{0}nm", googleProduct.position, counter), googleProduct.name },
                { string.Format("il{1}pi{0}ca", googleProduct.position, counter), googleProduct.category },
                { string.Format("il{1}pi{0}ps", googleProduct.position, counter), googleProduct.position.ToString() },
                { string.Format("il{1}pi{0}pr", googleProduct.position, counter), googleProduct.price.ToString("F") },
                { string.Format("il{1}pi{0}va", googleProduct.position, counter), googleProduct.publisher }
            };
        }


        private void SendImpressions(IEnumerable<GoogleProduct> googleProducts, string pageTitle)
        {
            try
            {
                var postData = GetBasePostData("event");
                postData.Add("ec", "Impression");
                postData.Add("ea", "List");
                postData.Add("il1nm", pageTitle);

                var postDataBuilder = new StringBuilder()
                    .Append(postData
                        .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                            HttpUtility.UrlEncode(next.Value))));

                var baseByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());
                int currentByteCount;
                var productCounter = 0;

                foreach (var googleProduct in googleProducts)
                {
                    productCounter++;
                    var impressionDetail = GetImpressionProductDetail(googleProduct, 1);
                    var impressionDetailString = impressionDetail
                        .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                            HttpUtility.UrlEncode(next.Value)));

                    postDataBuilder.Append(impressionDetailString);
                    currentByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());
                    if (currentByteCount >= 8000 || productCounter >= MaxBatchSize)
                    {
                        //Remove Last Entry and Send
                        postDataBuilder.Remove(postDataBuilder.Length - impressionDetailString.Length,
                            impressionDetailString.Length);

                        SendData(postDataBuilder.ToString());
                        //Rebuild the base
                        postDataBuilder = new StringBuilder()
                            .Append(postData
                                .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                                    HttpUtility.UrlEncode(next.Value))));

                        postDataBuilder.Append(impressionDetailString);
                        productCounter = 0;
                    }
                }

                currentByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());

                if (currentByteCount > baseByteCount)
                {
                    SendData(postDataBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void LogBulkAddToCart(List<InstitutionResource> institutionResources, string pageTitle)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var institutionResource in institutionResources)
            {
                counter++;
                var practiceArea = institutionResource.PracticeAreas?.FirstOrDefault();
                var googleProduct = new GoogleProduct
                {
                    id = institutionResource.Isbn,
                    name = institutionResource.Title,
                    price = institutionResource.DiscountPrice,
                    category = practiceArea?.Name,
                    position = counter,
                    quantity = 1,
                    publisher = institutionResource.PublisherName
                };
                googleProducts.Add(googleProduct);
            }

            SendBulkAddToCart(googleProducts, pageTitle);
        }

        private void SendBulkAddToCart(List<GoogleProduct> googleProducts, string pageTitle)
        {
            try
            {
                var postData = GetBasePostData("event");
                postData.Add("ec", "Ecommerce");
                postData.Add("ea", "Add to Cart");
                postData.Add("pal", pageTitle);

                ProcessGoogleProducts(googleProducts, postData, new Dictionary<string, string> { { "pa", "add" } });
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }


        public void LogBulkRemoveFromCart(List<IOrderItem> orderItems, string pageTitle)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var orderItem in orderItems)
            {
                var resourceOrderItem = orderItem as IResourceOrderItem;
                var productOrderItem = orderItem as IProductOrderItem;
                var googleProduct = new GoogleProduct();

                if (resourceOrderItem != null)
                {
                    if (resourceOrderItem.CoreResource == null)
                    {
                        continue;
                    }

                    var practiceArea = resourceOrderItem.CoreResource.PracticeAreas?.FirstOrDefault();
                    googleProduct = new GoogleProduct
                    {
                        id = resourceOrderItem.CoreResource.Isbn,
                        name = resourceOrderItem.CoreResource.Title,
                        publisher = resourceOrderItem.CoreResource.Publisher?.Name,
                        price = resourceOrderItem.DiscountPrice,
                        category = practiceArea?.Name,
                        position = 1,
                        quantity = resourceOrderItem.NumberOfLicenses
                    };
                }

                if (productOrderItem != null && productOrderItem.Include)
                {
                    if (productOrderItem.Product == null)
                    {
                        continue;
                    }

                    googleProduct = new GoogleProduct
                    {
                        id = $"Product-{productOrderItem.Product.Id}",
                        name = productOrderItem.Product.Name,
                        price = productOrderItem.DiscountPrice,
                        position = 1,
                        quantity = productOrderItem.NumberOfLicenses > 0 ? productOrderItem.NumberOfLicenses : 1
                    };
                }

                googleProducts.Add(googleProduct);
            }

            SendBulkRemoveFromCart(googleProducts, pageTitle);
        }

        private void SendBulkRemoveFromCart(List<GoogleProduct> googleProducts, string pageTitle)
        {
            try
            {
                var postData = GetBasePostData("event");
                postData.Add("ec", "Ecommerce");
                postData.Add("ea", "Remove to Cart");
                postData.Add("pal", pageTitle);

                ProcessGoogleProducts(googleProducts, postData, new Dictionary<string, string> { { "pa", "remove" } });
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }


        public void LogCheckoutStep(List<IOrderItem> orderItems, int step)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var orderItem in orderItems)
            {
                var resourceOrderItem = orderItem as IResourceOrderItem;
                var productOrderItem = orderItem as IProductOrderItem;
                var googleProduct = new GoogleProduct();

                if (resourceOrderItem != null)
                {
                    if (resourceOrderItem.CoreResource == null)
                    {
                        continue;
                    }

                    var practiceArea = resourceOrderItem.CoreResource.PracticeAreas?.FirstOrDefault();
                    googleProduct = new GoogleProduct
                    {
                        id = resourceOrderItem.CoreResource.Isbn,
                        name = resourceOrderItem.CoreResource.Title,
                        publisher = resourceOrderItem.CoreResource.Publisher?.Name,
                        price = resourceOrderItem.DiscountPrice,
                        category = practiceArea?.Name,
                        position = 1,
                        quantity = resourceOrderItem.NumberOfLicenses
                    };
                }

                if (productOrderItem != null && productOrderItem.Include)
                {
                    if (productOrderItem.Product == null)
                    {
                        continue;
                    }

                    googleProduct = new GoogleProduct
                    {
                        id = $"Product-{productOrderItem.Product.Id}",
                        name = productOrderItem.Product.Name,
                        price = productOrderItem.DiscountPrice,
                        position = 1,
                        quantity = productOrderItem.NumberOfLicenses > 0 ? productOrderItem.NumberOfLicenses : 1
                    };
                }

                googleProducts.Add(googleProduct);
            }

            SendCheckoutStep(googleProducts, step);
        }

        public void LogCheckoutStep(WebOrderHistory orderHistory, int step)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var orderHistoryResource in orderHistory.OrderHistoryResources)
            {
                if (orderHistoryResource.Resource == null)
                {
                    continue;
                }

                counter++;

                var practiceArea = orderHistoryResource.Resource.PracticeAreas?.FirstOrDefault();
                var googleProduct = new GoogleProduct
                {
                    id = orderHistoryResource.Resource.Isbn,
                    name = orderHistoryResource.Resource.Title,
                    publisher = orderHistoryResource.Resource.PublisherName,
                    price = orderHistoryResource.DiscountPrice,
                    category = practiceArea?.Name,
                    position = counter,
                    quantity = orderHistoryResource.NumberOfLicenses
                };
                googleProducts.Add(googleProduct);
            }

            foreach (var orderHistoryProduct in orderHistory.OrderHistoryProducts)
            {
                if (orderHistoryProduct.Product == null)
                {
                    continue;
                }

                counter++;
                var googleProduct = new GoogleProduct
                {
                    id = $"Product-{orderHistoryProduct.Product.Id}",
                    name = orderHistoryProduct.Product.Name,
                    price = orderHistoryProduct.DiscountPrice,
                    position = counter,
                    quantity = orderHistoryProduct.NumberOfLicenses > 0 ? orderHistoryProduct.NumberOfLicenses : 1
                };
                googleProducts.Add(googleProduct);
            }


            SendCheckoutStep(googleProducts, step);
        }

        private void SendCheckoutStep(List<GoogleProduct> googleProducts, int step)
        {
            try
            {
                var postData = GetBasePostData("event");
                postData.Add("ec", "Ecommerce");
                postData.Add("ea", "Checkout");

                var test = new Dictionary<string, string>
                {
                    { "pa", "checkout" },
                    { "cos", step.ToString() }
                };
                ProcessGoogleProducts(googleProducts, postData, test);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void LogCartPurchase(WebOrderHistory orderHistory)
        {
            var googleProducts = new List<GoogleProduct>();
            var counter = 0;
            foreach (var orderHistoryResource in orderHistory.OrderHistoryResources)
            {
                if (orderHistoryResource.Resource == null)
                {
                    continue;
                }

                counter++;
                var practiceArea = orderHistoryResource.Resource.PracticeAreas?.FirstOrDefault();
                var googleProduct = new GoogleProduct
                {
                    id = orderHistoryResource.Resource.Isbn,
                    name = orderHistoryResource.Resource.Title,
                    publisher = orderHistoryResource.Resource.PublisherName,
                    price = orderHistoryResource.DiscountPrice,
                    category = practiceArea?.Name,
                    position = counter,
                    quantity = orderHistoryResource.NumberOfLicenses
                };
                googleProducts.Add(googleProduct);
            }

            foreach (var orderHistoryProduct in orderHistory.OrderHistoryProducts)
            {
                if (orderHistoryProduct.Product == null)
                {
                    continue;
                }

                counter++;
                var googleProduct = new GoogleProduct
                {
                    id = $"Product-{orderHistoryProduct.Product.Id}",
                    name = orderHistoryProduct.Product.Name,
                    price = orderHistoryProduct.DiscountPrice,
                    position = counter,
                    quantity = orderHistoryProduct.NumberOfLicenses > 0 ? orderHistoryProduct.NumberOfLicenses : 1
                };
                googleProducts.Add(googleProduct);
            }

            var googlePurchase = new GooglePurchase
            {
                id = orderHistory.OrderNumber,
                revenue = orderHistory.OrderTotal,
                tax = 0,
                shipping = 0,
                coupon = orderHistory.PromotionCode
            };
            SendCartPurchase(googleProducts, googlePurchase);
        }

        private void SendCartPurchase(List<GoogleProduct> googleProducts, GooglePurchase googlePurchase)
        {
            try
            {
                var postData = GetBasePostData("event");
                postData.Add("ec", "Ecommerce");
                postData.Add("ea", "Purchase");

                var test = new Dictionary<string, string> { { "pa", "purchase" } };
                ProcessGoogleProducts(googleProducts, postData, test, googlePurchase);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private void ProcessGoogleProducts(IEnumerable<GoogleProduct> googleProducts,
            Dictionary<string, string> postData, Dictionary<string, string> addPerProduct,
            GooglePurchase googlePurchase = null)
        {
            var postDataBuilder = new StringBuilder()
                .Append(postData
                    .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                        HttpUtility.UrlEncode(next.Value))));


            if (googlePurchase != null)
            {
                var postData2 = new Dictionary<string, string>
                {
                    { "ti", googlePurchase.id },
                    { "ts", googlePurchase.shipping.ToString("F") },
                    { "tt", googlePurchase.tax.ToString("F") }
                };

                var productDetailsString = postData2
                    .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                        HttpUtility.UrlEncode(next.Value)));

                postDataBuilder.Append(productDetailsString);
            }


            var baseByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());
            int currentByteCount;
            var counter = 1;

            //decimal total = 0;

            foreach (var googleProduct in googleProducts)
            {
                var productDetails = GetProductDetail(googleProduct, counter);
                //Append in product coupon if it applies
                if (googlePurchase != null && !string.IsNullOrWhiteSpace(googlePurchase.coupon))
                {
                    productDetails.Add($"pr{counter}cc", googlePurchase.coupon);
                }

                if (addPerProduct != null && addPerProduct.Any())
                {
                    foreach (var keyValuePair in addPerProduct)
                    {
                        productDetails.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }


                var productDetailsString = productDetails
                    .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                        HttpUtility.UrlEncode(next.Value)));

                postDataBuilder.Append(productDetailsString);

                currentByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());

                if (currentByteCount >= 8000 || counter >= MaxBatchSize)
                {
                    //Remove Last Entry and Send
                    postDataBuilder.Remove(postDataBuilder.Length - productDetailsString.Length,
                        productDetailsString.Length);

                    SendData(postDataBuilder.ToString());
                    //Rebuild the base
                    postDataBuilder = new StringBuilder()
                        .Append(postData
                            .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                                HttpUtility.UrlEncode(next.Value))));

                    if (googlePurchase != null)
                    {
                        postDataBuilder.Append($"&ti={googlePurchase.id}");
                    }

                    postDataBuilder.Append(productDetailsString);
                }

                counter++;
            }

            currentByteCount = Encoding.UTF8.GetByteCount(postDataBuilder.ToString());

            if (currentByteCount > baseByteCount)
            {
                SendData(postDataBuilder.ToString());
            }
        }


        private static Dictionary<string, string> GetProductDetail(GoogleProduct product, string action,
            int productIndex)
        {
            return new Dictionary<string, string>
            {
                { $"pr{productIndex}id", product.id },
                { $"pr{productIndex}nm", product.name },
                { $"pr{productIndex}ca", product.category },
                { $"pr{productIndex}pr", product.price.ToString("F") },
                { $"pr{productIndex}qt", product.quantity.ToString() },
                { $"pr{productIndex}ps", (product.position == 0 ? 1 : product.position).ToString() },
                { $"pr{productIndex}va", product.publisher },
                { "pa", action }
            };
        }

        private static Dictionary<string, string> GetProductDetail(GoogleProduct product, int productIndex)
        {
            return new Dictionary<string, string>
            {
                { $"pr{productIndex}id", product.id },
                { $"pr{productIndex}nm", product.name },
                { $"pr{productIndex}ca", product.category },
                { $"pr{productIndex}pr", product.price.ToString("F") },
                { $"pr{productIndex}qt", product.quantity.ToString() },
                { $"pr{productIndex}ps", (product.position == 0 ? 1 : product.position).ToString() },
                { $"pr{productIndex}va", product.publisher }
            };
        }

        private Dictionary<string, string> GetBasePostData(string type)
        {
            var postData = new Dictionary<string, string>
            {
                { "v", "1" },
                { "tid", _clientSettings.GoogleAnalyticsAccount },
                { "cid", GetUserCookieValue() },
                { "t", type }
            };

            if (type.ToLower() == "pageview")
            {
                postData.Add("dh", HttpContext.Current.Request.Url.Host);
                postData.Add("dp", HttpContext.Current.Request.Path);
            }

            return postData;
        }


        private static string GetUserCookieValue()
        {
            var cookie = HttpContext.Current.Request.Cookies["User_UUID_R2library"];
            if (cookie == null)
            {
                var uuid = Guid.NewGuid();
                cookie = new HttpCookie("User_UUID_R2library", uuid.ToString());
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            return cookie.Value;
        }

        private void SendData(Dictionary<string, string> postData)
        {
            var postDataString = postData
                .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                    HttpUtility.UrlEncode(next.Value)));
            SendData(postDataString);
        }

        private void SendData(string postDataString)
        {
            try
            {
                var googleRequestData = new GoogleRequestData
                {
                    RequestData = postDataString,
                    SentFromUrl = HttpContext.Current.Request.Url.ToString(),
                    UrlToSendData = _clientSettings.GoogleAnalyticsUrl,
                    UserAgent = HttpContext.Current.Request.UserAgent,
                    MessageId = GoogleRequestData.CreateMessageId(),
                    Timestamp = GoogleRequestData.CreateTimestamp(),
                    Server = HttpContext.Current.Server.MachineName //Environment.MachineName
                };

                _log.InfoFormat("(GoogleRequestData) -- {0}", googleRequestData.ToDebugString());

                _analyticsQueueService.WriteDataToMessageQueue(googleRequestData);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }
    }
}