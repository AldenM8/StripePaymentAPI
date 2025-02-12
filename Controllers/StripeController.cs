using Microsoft.AspNetCore.Mvc;
using StripePaymentAPI.Services;

namespace StripePaymentAPI.Controllers
{
    [Route("api/stripe")]
    [ApiController]
    public class StripeController : ControllerBase
    {

        private readonly StripeService _stripeService;

        public StripeController(StripeService stripeService)
        {
            _stripeService = stripeService;
        }


        /// <summary>
        /// 創建 PaymentIntent（前端需要 Stripe.js 來處理付款）
        /// </summary>
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "金額必須大於 0。" });
            }

            var clientSecret = await _stripeService.CreatePaymentIntent(request.Amount, request.Currency);
            return Ok(new { clientSecret });
        }

        /// <summary>
        /// 創建 Checkout Session（Stripe 託管付款頁面）
        /// </summary>
        /// <summary>
        /// 創建 Checkout Session（Stripe 託管付款頁面）
        /// </summary>
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] List<ProductRequest> products)
        {
            if (products == null || !products.Any())
            {
                return BadRequest(new { message = "請提供至少一個商品。" });
            }

            var sessionUrl = await _stripeService.CreateCheckoutSession(products);
            if (string.IsNullOrEmpty(sessionUrl))
            {
                return StatusCode(500, new { message = "創建支付會話失敗。" });
            }

            return Ok(new { url = sessionUrl });
        }

        /// <summary>
        /// 退款
        /// </summary>
        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundRequest request)
        {
            if (string.IsNullOrEmpty(request.PaymentIntentId))
            {
                return BadRequest(new { message = "必須提供有效的 PaymentIntentId。" });
            }

            var result = await _stripeService.RefundPayment(request.PaymentIntentId);
            return Ok(new { status = result });
        }

        /// <summary>
        /// 查詢 Checkout Session 狀態
        /// </summary>
        [HttpGet("checkout-session/{sessionId}")]
        public async Task<IActionResult> GetCheckoutSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(new { message = "SessionId 不能為空。" });
            }

            var session = await _stripeService.GetCheckoutSession(sessionId);
            return Ok(session);
        }

        /// <summary>
        /// 查詢 PaymentIntent 狀態
        /// </summary>
        [HttpGet("payment-intent/{paymentIntentId}")]
        public async Task<IActionResult> GetPaymentIntentStatus(string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                return BadRequest(new { message = "PaymentIntentId 不能為空。" });
            }

            var paymentIntent = await _stripeService.GetPaymentIntentStatus(paymentIntentId);
            return Ok(paymentIntent);
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class RefundRequest
    {
        public string PaymentIntentId { get; set; }
    }
    public class ProductRequest
    {
        public string Name { get; set; }  // 商品名稱
        public decimal Price { get; set; } // 商品價格
        public string Currency { get; set; } // 貨幣（如 "usd", "twd"）
        public int Quantity { get; set; } // 數量
    }

}
