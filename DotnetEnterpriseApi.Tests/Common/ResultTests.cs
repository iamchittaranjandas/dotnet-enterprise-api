using DotnetEnterpriseApi.Application.Common.Models;
using FluentAssertions;

namespace DotnetEnterpriseApi.Tests.Common
{
    public class ResultTests
    {
        [Fact]
        public void Success_ReturnsIsSuccessTrue()
        {
            var result = Result.Success("Done");

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Done");
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Success_DefaultMessage()
        {
            var result = Result.Success();

            result.Message.Should().Be("Operation completed successfully");
        }

        [Fact]
        public void Failure_ReturnsIsSuccessFalse()
        {
            var result = Result.Failure("Something went wrong", "Error1", "Error2");

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Something went wrong");
            result.Errors.Should().Contain("Error1");
            result.Errors.Should().Contain("Error2");
        }

        [Fact]
        public void Failure_WithErrorsArray_UsesDefaultMessage()
        {
            var result = Result.Failure(new[] { "Error1" });

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Operation failed");
            result.Errors.Should().Contain("Error1");
        }

        [Fact]
        public void GenericSuccess_ContainsData()
        {
            var result = Result<string>.Success("data", "Done");

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be("data");
            result.Message.Should().Be("Done");
        }

        [Fact]
        public void GenericFailure_DataIsNull()
        {
            var result = Result<string>.Failure("Error");

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Error");
        }
    }
}
