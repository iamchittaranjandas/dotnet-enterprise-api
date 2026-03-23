using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DotnetEnterpriseApi.Tests.Validators
{
    public class CreateTaskCommandValidatorTests
    {
        private readonly CreateTaskCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new CreateTaskCommand { Title = "Valid Title", Description = "Valid Description" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyTitle_ShouldFail(string? title)
        {
            var command = new CreateTaskCommand { Title = title!, Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validate_TitleExceeds200Chars_ShouldFail()
        {
            var command = new CreateTaskCommand { Title = new string('A', 201), Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyDescription_ShouldFail(string? description)
        {
            var command = new CreateTaskCommand { Title = "Valid", Description = description! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_DescriptionExceeds1000Chars_ShouldFail()
        {
            var command = new CreateTaskCommand { Title = "Valid", Description = new string('A', 1001) };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_TitleAt200Chars_ShouldPass()
        {
            var command = new CreateTaskCommand { Title = new string('A', 200), Description = "Valid" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }
    }
}
