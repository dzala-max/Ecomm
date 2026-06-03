using EcommAPI.Entities;

namespace EcommAPI.Infra.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Product> Products { get; }

        IRepository<Order> Orders { get; }

        IRepository<OrderItem> OrderItems { get; }

        IRepository<Payment> Payments { get; }

        Task<int> CommitAsync();
    }
}
