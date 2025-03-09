using Dapper;
using Domain.DTOs;
using Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _connection;

        public ProductRepository(AuthDbContext context)
        {
            _connection = context.CreateConnection();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            return await _connection.QuerySingleAsync<Product>(
                "ProductOperations",
                new
                {
                    Action = "Create",
                    product.Id,
                    product.Name,
                    product.Price,
                    product.UserId,
                    product.CreatedAt
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _connection.QueryAsync<Product>(
                "ProductOperations",
                new { Action = "GetAll" },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            return await _connection.QuerySingleAsync<Product>(
                "ProductOperations",
                new
                {
                    Action = "Update",
                    Id = id,
                    product.Name,
                    product.Price
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _connection.ExecuteScalarAsync<int>(
                "ProductOperations",
                new { Action = "Delete", Id = id },
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }
    }
}


