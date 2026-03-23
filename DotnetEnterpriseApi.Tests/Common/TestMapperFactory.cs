using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetEnterpriseApi.Tests.Common
{
    public static class TestMapperFactory
    {
        public static IMapper Create()
        {
            var configExpr = new MapperConfigurationExpression();
            configExpr.AddProfile<MappingProfile>();
            return new MapperConfiguration(configExpr, NullLoggerFactory.Instance).CreateMapper();
        }
    }
}
