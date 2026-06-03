using EcommAPI.Exceptions;
using EcommDTO;
using EcommServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            PlaceOrderRequest request)
        {
            try
            {
                var orderId =
                    await _service.PlaceOrderAsync(request);

                return Ok(new
                {
                    OrderId = orderId
                });
            }
            catch (InsufficientStockException ex)
            {
                return Conflict(new
                {
                    Message = "Insufficient Stock",
                    ex.Failures
                });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }
    }
}
