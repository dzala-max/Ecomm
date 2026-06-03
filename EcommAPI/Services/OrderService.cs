using EcommAPI.Data;
using EcommAPI.Entities;
using EcommAPI.Exceptions;
using EcommAPI.Infra.Repositories.Interfaces;
using EcommDTO;
using EcommServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommServices
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IProductRepository _productRepository;

        //public OrderService(
        //    AppDbContext context,
        //    IProductRepository productRepository)
        //{
        //    _context = context;
        //    _productRepository = productRepository;
        //}

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Task #1 Repository Pattern
        //public async Task<int> PlaceOrderAsync(PlaceOrderRequest request)
        //{
        //    var strategy = _context.Database.CreateExecutionStrategy();

        //    return await strategy.ExecuteAsync(async () =>
        //    {
        //        await using IDbContextTransaction transaction =
        //            await _context.Database.BeginTransactionAsync();

        //        try
        //        {
        //            var productIds = request.Items
        //                .Select(x => x.ProductId)
        //                .ToList();

        //            var products =
        //                await _productRepository.GetProductsByIdsAsync(productIds);

        //            //--------------------------------------------------
        //            // STEP 1 - Validate Products Exist
        //            //--------------------------------------------------

        //            var missingProducts = productIds
        //                .Except(products.Select(x => x.Id))
        //                .ToList();

        //            if (missingProducts.Any())
        //            {
        //                throw new Exception(
        //                    $"Products not found: {string.Join(",", missingProducts)}");
        //            }

        //            //--------------------------------------------------
        //            // STEP 2 - Validate Stock
        //            //--------------------------------------------------

        //            var failures = new List<StockFailure>();

        //            foreach (var item in request.Items)
        //            {
        //                var product =
        //                    products.First(x => x.Id == item.ProductId);

        //                if (product.StockQuantity < item.Quantity)
        //                {
        //                    failures.Add(new StockFailure
        //                    {
        //                        ProductId = product.Id,
        //                        ProductName = product.Name,
        //                        AvailableStock = product.StockQuantity,
        //                        RequestedQuantity = item.Quantity
        //                    });
        //                }
        //            }

        //            if (failures.Any())
        //            {
        //                throw new InsufficientStockException(failures);
        //            }

        //            //--------------------------------------------------
        //            // SAVEPOINT
        //            //--------------------------------------------------

        //            await transaction.CreateSavepointAsync("BeforeStockUpdate");

        //            //--------------------------------------------------
        //            // STEP 3 - Deduct Stock
        //            //--------------------------------------------------

        //            foreach (var item in request.Items)
        //            {
        //                var product =
        //                    products.First(x => x.Id == item.ProductId);

        //                product.StockQuantity -= item.Quantity;
        //            }

        //            await _context.SaveChangesAsync();

        //            //--------------------------------------------------
        //            // STEP 4 - Create Order
        //            //--------------------------------------------------

        //            var order = new Order
        //            {
        //                CustomerId = request.CustomerId,
        //                OrderDate = DateTime.UtcNow,
        //                Status = "Created"
        //            };

        //            await _context.Orders.AddAsync(order);

        //            await _context.SaveChangesAsync();

        //            //--------------------------------------------------
        //            // STEP 5 - Create Order Items
        //            //--------------------------------------------------

        //            var orderItems = request.Items.Select(item =>
        //            {
        //                var product =
        //                    products.First(x => x.Id == item.ProductId);

        //                return new OrderItem
        //                {
        //                    OrderId = order.Id,
        //                    ProductId = product.Id,
        //                    Quantity = item.Quantity,
        //                    UnitPrice = product.Price
        //                };
        //            });

        //            await _context.OrderItems.AddRangeAsync(orderItems);

        //            await _context.SaveChangesAsync();

        //            //--------------------------------------------------
        //            // STEP 6 - Create Payment
        //            //--------------------------------------------------

        //            var payment = new Payment
        //            {
        //                OrderId = order.Id,
        //                Amount = orderItems.Sum(x =>
        //                    x.Quantity * x.UnitPrice),
        //                Status = "Pending",
        //                CreatedAt = DateTime.UtcNow
        //            };

        //            await _context.Payments.AddAsync(payment);

        //            await _context.SaveChangesAsync();

        //            //--------------------------------------------------
        //            // COMMIT
        //            //--------------------------------------------------

        //            await transaction.CommitAsync();

        //            return order.Id;
        //        }
        //        catch (InsufficientStockException)
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }
        //        catch (Exception)
        //        {
        //            try
        //            {
        //                await transaction.RollbackToSavepointAsync(
        //                    "BeforeStockUpdate");
        //            }
        //            catch
        //            {
        //            }

        //            await transaction.RollbackAsync();

        //            throw;
        //        }
        //    });
        //} 
        #endregion

        #region Task #2 Unit OF Work

        //public async Task<int> PlaceOrderAsync(PlaceOrderRequest request)
        //{

        //    int retryCount = 0;
        //    const int maxRetries = 3;

        //    var productIds = request.Items.Select(x => x.ProductId).ToList();

        //    var products = await _unitOfWork.Products
        //        .FindAsync(p => productIds.Contains(p.Id));

        //    // Validate missing products
        //    var missing = productIds
        //        .Except(products.Select(p => p.Id))
        //        .ToList();

        //    if (missing.Any())
        //        throw new Exception($"Missing products: {string.Join(",", missing)}");

        //    // Stock validation
        //    var failures = new List<StockFailure>();

        //    foreach (var item in request.Items)
        //    {
        //        var product = products.First(p => p.Id == item.ProductId);

        //        if (product.StockQuantity < item.Quantity)
        //        {
        //            failures.Add(new StockFailure
        //            {
        //                ProductId = product.Id,
        //                ProductName = product.Name,
        //                AvailableStock = product.StockQuantity,
        //                RequestedQuantity = item.Quantity
        //            });
        //        }
        //    }

        //    if (failures.Any())
        //        throw new InsufficientStockException(failures);

        //    // Deduct stock
        //    foreach (var item in request.Items)
        //    {
        //        var product = products.First(p => p.Id == item.ProductId);
        //        product.StockQuantity -= item.Quantity;

        //        _unitOfWork.Products.Update(product);
        //    }

        //    // Create Order
        //    var order = new Order
        //    {
        //        CustomerId = request.CustomerId,
        //        OrderDate = DateTime.UtcNow,
        //        Status = "Created"
        //    };

        //    await _unitOfWork.Orders.AddAsync(order);
        //    await _unitOfWork.CommitAsync(); // order.Id generated

        //    // Create OrderItems
        //    foreach (var item in request.Items)
        //    {
        //        var product = products.First(p => p.Id == item.ProductId);

        //        await _unitOfWork.OrderItems.AddAsync(new OrderItem
        //        {
        //            OrderId = order.Id,
        //            ProductId = product.Id,
        //            Quantity = item.Quantity,
        //            UnitPrice = product.Price
        //        });
        //    }

        //    // Create Payment
        //    var payment = new Payment
        //    {
        //        OrderId = order.Id,
        //        Amount = request.Items.Sum(i =>
        //            products.First(p => p.Id == i.ProductId).Price * i.Quantity),
        //        Status = "Pending",
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    await _unitOfWork.Payments.AddAsync(payment);

        //    await _unitOfWork.CommitAsync();

        //    return order.Id;
        //} 
        #endregion





        #region Task #3  Concurrency  Logic Implemented
        /// <summary>
        /// Optimistic concurrency ensures that when multiple users try to
        /// update the same Product stock simultaneously:
        /// 
        /// - EF Core tracks RowVersion (timestamp column)
        /// - Each update checks if RowVersion matches database value
        /// - If another transaction updated the row first,
        ///   DbUpdateConcurrencyException is thrown
        /// 
        /// This prevents race conditions where stock could be
        /// over-sold due to concurrent order placement.
        /// </summary>
        public async Task<int> PlaceOrderAsync(PlaceOrderRequest request)
        {
            int retryCount = 0;
            const int maxRetries = 3;

            while (true)
            {
                try
                {
                    var productIds = request.Items.Select(x => x.ProductId).ToList();

                    var products = await _unitOfWork.Products
                        .FindAsync(p => productIds.Contains(p.Id));

                    // STEP 1: Validate stock
                    var failures = new List<StockFailure>();

                    foreach (var item in request.Items)
                    {
                        var product = products.First(p => p.Id == item.ProductId);

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
                        throw new InsufficientStockException(failures);

                    // STEP 2: Deduct stock
                    foreach (var item in request.Items)
                    {
                        var product = products.First(p => p.Id == item.ProductId);
                        product.StockQuantity -= item.Quantity;

                        _unitOfWork.Products.Update(product);
                    }

                    // STEP 3: Create Order
                    var order = new Order
                    {
                        CustomerId = request.CustomerId,
                        OrderDate = DateTime.UtcNow,
                        Status = "Created"
                    };

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.CommitAsync();

                    // STEP 4: OrderItems
                    foreach (var item in request.Items)
                    {
                        var product = products.First(p => p.Id == item.ProductId);

                        await _unitOfWork.OrderItems.AddAsync(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price
                        });
                    }

                    await _unitOfWork.CommitAsync();

                    return order.Id;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        // After retries, return conflict
                        throw new Exception(
                            "Concurrency conflict: Unable to complete order after multiple retries.");
                    }

                    // Reload latest values from DB
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Product)
                        {
                            var databaseValues = await entry.GetDatabaseValuesAsync();

                            if (databaseValues == null)
                                throw new Exception("Product deleted by another user.");

                            entry.OriginalValues.SetValues(databaseValues);
                            entry.CurrentValues.SetValues(databaseValues);
                        }
                    }

                    // Retry loop continues automatically
                }
            }
        } 
        #endregion

    }
}
