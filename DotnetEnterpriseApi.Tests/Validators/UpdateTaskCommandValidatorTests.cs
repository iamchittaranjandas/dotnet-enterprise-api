using DotnetEnterpriseApi.Application.Features.Tasks.Commands.UpdateTask;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DotnetEnterpriseApi.Tests.Validators
{
    public class UpdateTaskCommandValidatorTests
    {
        private readonly UpdateTaskCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new UpdateTaskCommand { Id = 1, Title = "Valid", Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            var command = new UpdateTaskCommand { Id = id, Title = "Valid", Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Validate_EmptyTitle_ShouldFail()
        {
            var command = new UpdateTaskCommand { Id = 1, Title = "", Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validate_TitleExceeds200Chars_ShouldFail()
        {
            var command = new UpdateTaskCommand { Id = 1, Title = new string('A', 201), Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validate_EmptyDescription_ShouldFail()
        {
            var command = new UpdateTaskCommand { Id = 1, Title = "Valid", Description = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_DescriptionExceeds1000Chars_ShouldFail()
        {
            var command = new UpdateTaskCommand { Id = 1, Title = "Valid", Description = new string('A', 1001) };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
