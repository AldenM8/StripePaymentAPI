using Stripe;
using Stripe.Checkout;
using StripePaymentAPI.Controllers;

namespace StripePaymentAPI.Services
{
    public class StripeService
    {
        //自訂付款頁面用 暫時不需要
        //public async Task<string> CreatePaymentIntent(decimal amount, string currency)
        //{
        //    var paymentIntentService = new PaymentIntentService();
        //    var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        //    {
        //        Amount = (long)(amount * 100), // Stripe 以 "分" 為單位
        //        Currency = currency, // USD, TWD, etc.
        //        PaymentMethodTypes = new List<string> { "card" },
        //    });

        //    return paymentIntent.ClientSecret; // 傳回給前端
        //}
        public async Task<string> CreateCheckoutSession(List<ProductRequest> products)
        {
            try
            {
                if (products == null || !products.Any())
                {
                    return null; // 讓 Controller 處理 BadRequest
                }

                var lineItems = products.Select(p => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(p.Price * 100),
                        Currency = p.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = p.Name
                        }
                    },
                    Quantity = p.Quantity
                }).ToList();

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = "https://google.com",
                    CancelUrl = "https://google.com",
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);
                return session.Url;
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe API 錯誤: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"其他錯誤: {ex.Message}");
                return null;
            }
        }
        public async Task<string> CreateSubscription(string customerEmail, string priceId)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                Price = priceId, // 這是你在 Stripe 設定的訂閱 Price ID
                                Quantity = 1
                            }
                        },
                    Mode = "subscription", // 設定為訂閱模式
                    CustomerEmail = customerEmail,
                    SuccessUrl = "https://google.com",
                    CancelUrl = "https://google.com"
                };


                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return session.Url;
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe 訂閱錯誤: {ex.Message}");
                return null;
            }
        }

        public async Task<Session> GetCheckoutSession(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }
        public async Task<PaymentIntent> GetPaymentIntentStatus(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }
        public async Task<string> RefundPayment(string paymentIntentId)
        {
            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId, // 退款目標付款
            });

            return refund.Status; // 回傳退款狀態
        }

    }
}
