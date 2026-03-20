using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly IUserRepository _userRepository;

        public RegisterCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _userRepository.UserExistsAsync(request.Email);

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

            var created = await _userRepository.AddAsync(user);

            var response = new RegisterResponse
            {
                Id = created.Id,
                UserName = created.UserName,
                Email = created.Email,
                Message = "User registered successfully"
            };

            return Result<RegisterResponse>.Success(response, "User registered successfully");
        }
    }
}
