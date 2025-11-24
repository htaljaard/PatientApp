
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FastEndpoints;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientApp.SharedKernel.Auth;
using UserService.API.Domain;


namespace UserService.API.EndPoints;

internal sealed class Login(UserManager<ApplicationUser> userManager, IOptions<JwtOptions> jwtOptions) : Endpoint<LoginRequest, LoginResponse>
{

    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    public override void Configure()
    {
        Post("/api/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(req.UserName);

        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
        {
            ThrowError("Username or password is incorrect", 400);
            return;
        }

        SecurityToken token = await generateTokenAsync(user);

        await Send.OkAsync(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)), ct);
    }

    private async Task<SecurityToken> generateTokenAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email??string.Empty),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            SigningCredentials = creds,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience

        };

        SecurityToken token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);

        return token;
    }
}

internal record LoginRequest(string UserName, string Password);

internal record LoginResponse(string Token);

internal class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Username or password is incorrect");
    }
}