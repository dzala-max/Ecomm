using EcommAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommAPI.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<AppDbContext>>());

            if (context.Products.Any())
                return;

            context.Products.AddRange(
                new Product
                {
                    Name = "Laptop",
                    Price = 50000,
                    StockQuantity = 10
                },
                new Product
                {
                    Name = "Mouse",
                    Price = 500,
                    StockQuantity = 50
                });

            context.SaveChanges();
        }
    }
}
