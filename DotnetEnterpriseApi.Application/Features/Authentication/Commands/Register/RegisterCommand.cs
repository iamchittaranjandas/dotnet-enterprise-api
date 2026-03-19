using DotnetEnterpriseApi.Application.Common.Models;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommand : IRequest<Result<RegisterResponse>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
