using DotnetEnterpriseApi.Application.Common.Models;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.DeleteTask
{
    public class DeleteTaskCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
}
