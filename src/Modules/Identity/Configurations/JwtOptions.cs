﻿using Microsoft.IdentityModel.Tokens;

namespace Identity.Configurations;

public class JwtOptions
{
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
}
