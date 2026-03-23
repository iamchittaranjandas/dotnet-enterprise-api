using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Login;
using FluentValidation.TestHelper;

namespace DotnetEnterpriseApi.Tests.Validators
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new LoginCommand { Email = "test@example.com", Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyEmail_ShouldFail(string? email)
        {
            var command = new LoginCommand { Email = email!, Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_InvalidEmailFormat_ShouldFail()
        {
            var command = new LoginCommand { Email = "notanemail", Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyPassword_ShouldFail(string? password)
        {
            var command = new LoginCommand { Email = "test@test.com", Password = password! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
