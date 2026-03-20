using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Login;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
    {
        public string Token { get; set; } = string.Empty;
    }
}
