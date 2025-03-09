using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResult> RegisterAsync(string email, string password, string fullName)
        {
            try
            {
                // Create user with plain password - hashing happens in DB
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = fullName,
                    CreatedAt = DateTime.UtcNow
                };

                // Pass plain password to repository
                var user = await _userRepository.CreateWithPasswordAsync(newUser, password);
                
                if (user == null)
                    return new AuthResult { Errors = new[] { "User already exists" } };
                    
                return GenerateAuthResult(user);
            }
            catch (Exception ex)
            {
                return new AuthResult { Errors = new[] { $"Registration failed: {ex.Message}" } };
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var sw = Stopwatch.StartNew();
            const int minDelay = 2000; // 2 second minimum delay
            
            try
            {
                // Pass plain credentials to repository - all verification in DB
                var user = await _userRepository.ValidateCredentialsAsync(email, password);
                
                var result = user != null
                    ? GenerateAuthResult(user)
                    : new AuthResult { Errors = new[] { "Invalid credentials" } };

                // Enforce constant response time
                var elapsed = sw.ElapsedMilliseconds;
                if (elapsed < minDelay)
                {
                    await Task.Delay((int)(minDelay - elapsed));
                }

                return result;
            }
            catch
            {
                await Task.Delay(minDelay);
                return new AuthResult { Errors = new[] { "Invalid credentials" } };
            }
        }

        private AuthResult GenerateAuthResult(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtSettings.Audience,
                Issuer = _jwtSettings.Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
