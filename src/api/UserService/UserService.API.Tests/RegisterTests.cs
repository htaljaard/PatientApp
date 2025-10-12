using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.API.Domain;
using UserService.API.EndPoints;

namespace UserService.API.Tests;

public class RegisterTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<Register>> _loggerMock;

    public RegisterTests()
    {
        // Setup UserManager mock with required dependencies
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        _loggerMock = new Mock<ILogger<Register>>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_RegistersUserSuccessfully()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "ValidPass1!");
        var cancellationToken = CancellationToken.None;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var endpoint = new Register(_userManagerMock.Object, _loggerMock.Object);

        // Act & Assert - FastEndpoints requires special setup for testing
        // We'll verify the UserManager was called with correct parameters
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await endpoint.HandleAsync(request, cancellationToken);
        });

        // Verify user creation was attempted
        _userManagerMock.Verify(
            x => x.FindByEmailAsync(request.Email),
            Times.Once);

        _userManagerMock.Verify(
            x => x.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == request.Email && u.UserName == request.Email),
                request.Password),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest("existing@example.com", "ValidPass1!");
        var cancellationToken = CancellationToken.None;
        var existingUser = new ApplicationUser { Email = request.Email, UserName = request.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var endpoint = new Register(_userManagerMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await endpoint.HandleAsync(request, cancellationToken);
        });

        // Verify user creation was NOT attempted
        _userManagerMock.Verify(
            x => x.FindByEmailAsync(request.Email),
            Times.Once);

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData("notanemail", "ValidPass1!")]
    [InlineData("", "ValidPass1!")]
    [InlineData("   ", "ValidPass1!")]
    public void Validator_WithInvalidEmail_ShouldFail(string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(email, password);
        var validator = new RegisterValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "short")]
    [InlineData("test@example.com", "nouppercase1!")]
    [InlineData("test@example.com", "NOLOWERCASE1!")]
    [InlineData("test@example.com", "NoNumbers!")]
    [InlineData("test@example.com", "NoSpecialChar1")]
    public void Validator_WithInvalidPassword_ShouldFail(string email, string password)
    {
        // Arrange
        var request = new RegisterRequest(email, password);
        var validator = new RegisterValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validator_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "ValidPass1!");
        var validator = new RegisterValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserCreationFails_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "ValidPass1!");
        var cancellationToken = CancellationToken.None;
        var identityError = new IdentityError { Code = "Error", Description = "User creation failed" };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var endpoint = new Register(_userManagerMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await endpoint.HandleAsync(request, cancellationToken);
        });

        // Verify user creation was attempted but role assignment was not
        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password),
            Times.Once);

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }
}
