using DotnetEnterpriseApi.Application.Common.Behaviours;
using DotnetEnterpriseApi.Application.Features.Tasks.AgentTools;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotnetEnterpriseApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            services.AddScoped<TaskAgentTools>();
            services.AddScoped<QueryAgentTools>();
            services.AddScoped<MutationAgentTools>();

            return services;
        }
    }
}
