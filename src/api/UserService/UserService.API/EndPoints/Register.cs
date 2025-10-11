using System.Linq.Expressions;

using FastEndpoints;

using FluentValidation;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

using UserService.API.Domain;

namespace UserService.API.EndPoints;

internal sealed class Register(UserManager<ApplicationUser> userManager, ILogger<Register> logger) : Endpoint<RegisterRequest, Results<Ok<RegisterResponse>, BadRequest>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<Register> _logger = logger;

    public override void Configure()
    {
        Post("/api/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email ?? string.Empty);

        if (user is not null)
        {
            _logger.LogWarning("User with email {Email} already exists", req.Email);
            await Send.ResultAsync(TypedResults.BadRequest("Unable to register user"));
            return;
        }

        try
        {
            IdentityResult newUserRequest = await _userManager.CreateAsync(new ApplicationUser
            {
                Email = req.Email,
                UserName = req.Email
            }, req.Password);


            if (newUserRequest.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await Send.ResultAsync(TypedResults.Ok(new RegisterResponse(true)));
            }
            else
                await Send.ResultAsync(TypedResults.BadRequest(newUserRequest.Errors.FirstOrDefault()));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while registering user");

            throw;
        }
    }
}

internal sealed record RegisterRequest(string Email, string Password);

internal sealed record RegisterResponse(bool success);

internal sealed class RegisterValidator : Validator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[A-Z]").WithMessage("Password must contain at least one upper case letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lower case letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}