using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace StripePaymentAPI.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _webhookSecret;

        public StripeWebhookController(IConfiguration configuration)
        {
            _webhookSecret = configuration["Stripe:WebhookSecret"];
        }

        [HttpPost]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                Console.WriteLine($"收到 Stripe 事件: {stripeEvent.Type}");

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine($"成功付款：{paymentIntent?.Id}");
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe Webhook 錯誤: {ex.Message}");
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"其他 Webhook 錯誤: {ex.Message}");
                return BadRequest();
            }
        }

    }
}
