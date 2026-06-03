using EcommAPI.Data;
using EcommAPI.Entities;
using EcommAPI.Infra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcommAPI.Infra.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRepository<Product> Products { get; }
        public IRepository<Order> Orders { get; }
        public IRepository<OrderItem> OrderItems { get; }
        public IRepository<Payment> Payments { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Products = new Repository<Product>(context);
            Orders = new Repository<Order>(context);
            OrderItems = new Repository<OrderItem>(context);
            Payments = new Repository<Payment>(context);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();

                if (_transaction != null)
                    await _transaction.CommitAsync();

                return result;
            }
            catch
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();

                throw;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
