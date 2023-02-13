using Identity.Configurations;
using Identity.Dtos;
using Identity.Exceptions;
using Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity;

internal class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<IdentityService> _logger;
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _signingCredentials;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<IdentityService> logger,
        IOptions<JwtOptions> jwtOptions,
        SigningCredentials signingCredentials)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
        _signingCredentials = signingCredentials;
    }

    public async Task SignUp(UserSignUpInput input)
    {
        var identityUser = MapSignUpDtoToIdentityUser(input);
        _logger.LogInformation("User creating account: {username} / {email}", input.UserName, input.Email);
        var result = await _userManager.CreateAsync(identityUser, input.Password);
        if(!result.Succeeded)
            throw new IdentityException("User can't be created", result.Errors);
        _logger.LogInformation("User created successfully: {username} / {email}", input.UserName, input.Email);
        await _userManager.SetLockoutEnabledAsync(identityUser, false);
    }

    private static IdentityUser MapSignUpDtoToIdentityUser(UserSignUpInput input)
        => new()
        {
            UserName = input.UserName,
            Email = input.Email,
            EmailConfirmed = true
        };

    public async Task<UserLoginOutput> Login(UserLoginInput input)
    {
        var userByEmail = await _userManager.FindByEmailAsync(input.Email);
        if(userByEmail is null || userByEmail.UserName is null)
            throw new InvalidCredentialsException();
        var result = await _signInManager.PasswordSignInAsync(userByEmail.UserName, input.Password, false, false);
        if(!result.Succeeded)
            throw new InvalidCredentialsException();
        return await GenerateUserLoginOutput(userByEmail.UserName);
    }

    private async Task<UserLoginOutput> GenerateUserLoginOutput(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if(user is null) throw new IdentityException("After login user not found");
        var accessTokenClaims = await GetClaims(user, true);
        var refreshTokenClaims = await GetClaims(user);

        var accessTokenmExpirationDate = DateTime.Now.AddSeconds(_jwtOptions.AccessTokenExpiration);
        var refreshTokenmExpirationDate = DateTime.Now.AddSeconds(_jwtOptions.AccessTokenExpiration);

        var accessToken = CreateToken(accessTokenClaims, accessTokenmExpirationDate);
        var refreshToken = CreateToken(refreshTokenClaims, refreshTokenmExpirationDate);

        return new UserLoginOutput(
            true,
            accessToken,
            refreshToken,
            user.Id,
            user.UserName);
    }

    private async Task<IList<Claim>> GetClaims(IdentityUser user, bool addUserClaim = false)
    {
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Nbf, DateTime.Now.ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString())};
        if(addUserClaim) await AddUserClaims(user, claims);
        return claims;
    }

    private async Task AddUserClaims(IdentityUser user, List<Claim> claims)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userClaims);
        foreach(var role in roles) claims.Add(new("role", role));
    }

    private string CreateToken(IEnumerable<Claim> claims, DateTime expirationdate)
        => new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expirationdate,
                signingCredentials: _signingCredentials));
}
