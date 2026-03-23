using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register;
using FluentValidation.TestHelper;

namespace DotnetEnterpriseApi.Tests.Validators
{
    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password1"
            };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyUserName_ShouldFail(string? userName)
        {
            var command = new RegisterCommand { UserName = userName!, Email = "test@test.com", Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Validate_UserNameTooShort_ShouldFail()
        {
            var command = new RegisterCommand { UserName = "ab", Email = "test@test.com", Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Validate_UserNameTooLong_ShouldFail()
        {
            var command = new RegisterCommand { UserName = new string('a', 51), Email = "test@test.com", Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("notanemail")]
        [InlineData("missing@")]
        public void Validate_InvalidEmail_ShouldFail(string email)
        {
            var command = new RegisterCommand { UserName = "testuser", Email = email, Password = "Password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_EmailTooLong_ShouldFail()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = new string('a', 92) + "@test.com",
                Password = "Password1"
            };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyPassword_ShouldFail(string? password)
        {
            var command = new RegisterCommand { UserName = "testuser", Email = "test@test.com", Password = password! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_PasswordTooShort_ShouldFail()
        {
            var command = new RegisterCommand { UserName = "testuser", Email = "test@test.com", Password = "Ab1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_PasswordNoUppercase_ShouldFail()
        {
            var command = new RegisterCommand { UserName = "testuser", Email = "test@test.com", Password = "password1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_PasswordNoLowercase_ShouldFail()
        {
            var command = new RegisterCommand { UserName = "testuser", Email = "test@test.com", Password = "PASSWORD1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_PasswordNoNumber_ShouldFail()
        {
            var command = new RegisterCommand { UserName = "testuser", Email = "test@test.com", Password = "PasswordOnly" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Validate_PasswordTooLong_ShouldFail()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "test@test.com",
                Password = "A1" + new string('a', 99)
            };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
