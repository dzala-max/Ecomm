using EcommDTO;
using EcommServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Polly;
using Polly.Retry;

namespace EcommServices
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;

        public OrderService(
            AppDbContext context,
            IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<int> PlaceOrderAsync(PlaceOrderRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var productIds = request.Items
                        .Select(x => x.ProductId)
                        .ToList();

                    var products =
                        await _productRepository.GetProductsByIdsAsync(productIds);

                    //--------------------------------------------------
                    // STEP 1 - Validate Products Exist
                    //--------------------------------------------------

                    var missingProducts = productIds
                        .Except(products.Select(x => x.Id))
                        .ToList();

                    if (missingProducts.Any())
                    {
                        throw new Exception(
                            $"Products not found: {string.Join(",", missingProducts)}");
                    }

                    //--------------------------------------------------
                    // STEP 2 - Validate Stock
                    //--------------------------------------------------

                    var failures = new List<StockFailure>();

                    foreach (var item in request.Items)
                    {
                        var product =
                            products.First(x => x.Id == item.ProductId);

                        if (product.StockQuantity < item.Quantity)
                        {
                            failures.Add(new StockFailure
                            {
                                ProductId = product.Id,
                                ProductName = product.Name,
                                AvailableStock = product.StockQuantity,
                                RequestedQuantity = item.Quantity
                            });
                        }
                    }

                    if (failures.Any())
                    {
                        throw new InsufficientStockException(failures);
                    }

                    //--------------------------------------------------
                    // SAVEPOINT
                    //--------------------------------------------------

                    await transaction.CreateSavepointAsync("BeforeStockUpdate");

                    //--------------------------------------------------
                    // STEP 3 - Deduct Stock
                    //--------------------------------------------------

                    foreach (var item in request.Items)
                    {
                        var product =
                            products.First(x => x.Id == item.ProductId);

                        product.StockQuantity -= item.Quantity;
                    }

                    await _context.SaveChangesAsync();

                    //--------------------------------------------------
                    // STEP 4 - Create Order
                    //--------------------------------------------------

                    var order = new Order
                    {
                        CustomerId = request.CustomerId,
                        OrderDate = DateTime.UtcNow,
                        Status = "Created"
                    };

                    await _context.Orders.AddAsync(order);

                    await _context.SaveChangesAsync();

                    //--------------------------------------------------
                    // STEP 5 - Create Order Items
                    //--------------------------------------------------

                    var orderItems = request.Items.Select(item =>
                    {
                        var product =
                            products.First(x => x.Id == item.ProductId);

                        return new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price
                        };
                    });

                    await _context.OrderItems.AddRangeAsync(orderItems);

                    await _context.SaveChangesAsync();

                    //--------------------------------------------------
                    // STEP 6 - Create Payment
                    //--------------------------------------------------

                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Amount = orderItems.Sum(x =>
                            x.Quantity * x.UnitPrice),
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Payments.AddAsync(payment);

                    await _context.SaveChangesAsync();

                    //--------------------------------------------------
                    // COMMIT
                    //--------------------------------------------------

                    await transaction.CommitAsync();

                    return order.Id;
                }
                catch (InsufficientStockException)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                catch (Exception)
                {
                    try
                    {
                        await transaction.RollbackToSavepointAsync(
                            "BeforeStockUpdate");
                    }
                    catch
                    {
                    }

                    await transaction.RollbackAsync();

                    throw;
                }
            });
        }
    }
}
