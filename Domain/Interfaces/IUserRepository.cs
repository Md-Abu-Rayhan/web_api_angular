using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> ValidateCredentialsAsync(string email, string password);
        Task<User> CreateWithPasswordAsync(User user, string password);
    }
}
