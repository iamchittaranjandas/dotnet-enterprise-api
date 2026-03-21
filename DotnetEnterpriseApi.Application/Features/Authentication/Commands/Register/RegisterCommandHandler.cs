using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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

            var response = _mapper.Map<RegisterResponse>(created);
            response.Message = "User registered successfully";

            return Result<RegisterResponse>.Success(response, "User registered successfully");
        }
    }
}
