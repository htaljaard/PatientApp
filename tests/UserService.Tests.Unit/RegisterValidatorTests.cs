using FluentAssertions;
using UserService.API.EndPoints;

namespace UserService.Tests.Unit;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator;

    public RegisterValidatorTests()
    {
        _validator = new RegisterValidator();
    }

    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password1!");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid")]
    public async Task ValidateAsync_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var request = new RegisterRequest(email, "Password1!");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    public async Task ValidateAsync_WithEmptyPassword_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("pass1")]
    public async Task ValidateAsync_WithPasswordTooShort_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("6"));
    }

    [Theory]
    [InlineData("password1!")]
    [InlineData("nocapitals123!")]
    public async Task ValidateAsync_WithPasswordMissingUpperCase_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("upper case"));
    }

    [Theory]
    [InlineData("PASSWORD1!")]
    [InlineData("NOLOWERCASE123!")]
    public async Task ValidateAsync_WithPasswordMissingLowerCase_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lower case"));
    }

    [Theory]
    [InlineData("Password!")]
    [InlineData("NoNumbers!")]
    public async Task ValidateAsync_WithPasswordMissingNumber_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("number"));
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("NoSpecial123")]
    public async Task ValidateAsync_WithPasswordMissingSpecialCharacter_ShouldFail(string password)
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", password);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("special character"));
    }
}
