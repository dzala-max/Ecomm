using System.ComponentModel.DataAnnotations;

namespace EcommAPI.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }


        // 🔴 Concurrency token
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
