using EcommDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommServices.Interfaces
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(
            PlaceOrderRequest request);
    }
}
