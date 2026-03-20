using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            var token = GenerateJwtToken(user);

            var response = new LoginResponse
            {
                Token = token,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            return Result<LoginResponse>.Success(response, "Login successful");
        }

        private string GenerateJwtToken(Domain.Entities.AppUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
