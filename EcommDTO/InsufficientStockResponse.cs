using System;
using System.Collections.Generic;
using System.Text;

namespace EcommDTO
{
    public class InsufficientStockResponse
    {

        public List<StockFailure> FailedProducts { get; set; } = new();
    }

    public class StockFailure
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int AvailableStock { get; set; }

        public int RequestedQuantity { get; set; }
    }
}
