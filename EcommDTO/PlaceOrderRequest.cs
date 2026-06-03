using System;
using System.Collections.Generic;
using System.Text;

namespace EcommDTO
{
    public class PlaceOrderRequest
    {
        public int CustomerId { get; set; }

        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
