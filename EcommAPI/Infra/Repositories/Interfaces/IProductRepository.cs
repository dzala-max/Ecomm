using EcommAPI.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommAPI.Infra.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsByIdsAsync(List<int> ids);

    }
}
