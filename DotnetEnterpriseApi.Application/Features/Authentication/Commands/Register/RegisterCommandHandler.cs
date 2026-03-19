using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly IApplicationDbContext _context;

        public RegisterCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (userExists)
            {
                return Result<RegisterResponse>.Failure("User with this email already exists");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new RegisterResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Message = "User registered successfully"
            };

            return Result<RegisterResponse>.Success(response, "User registered successfully");
        }
    }
}
