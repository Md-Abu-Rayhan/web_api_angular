using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        public UserRepository(AuthDbContext context)
        {
            _connection = context.CreateConnection();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _connection.QueryFirstOrDefaultAsync<User>(
                "Users_GetByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<User> ValidateCredentialsAsync(string email, string password)
        {
            return await _connection.QueryFirstOrDefaultAsync<User>(
                "Users_ValidateCredentials",
                new { Email = email, Password = password },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<User> CreateWithPasswordAsync(User user, string password)
        {
            try
            {
                return await _connection.QueryFirstOrDefaultAsync<User>(
                    "Users_Create",
                    new { 
                        user.Id, 
                        user.Email, 
                        Password = password,  // Pass plain password
                        user.FullName, 
                        user.CreatedAt 
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return null; // User likely already exists
            }
        }
    }
}
