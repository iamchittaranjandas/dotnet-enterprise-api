using AutoMapper;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskItem, TaskResponse>();
            CreateMap<AppUser, RegisterResponse>();
        }
    }
}
