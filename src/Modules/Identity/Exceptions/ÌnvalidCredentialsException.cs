using Microsoft.AspNetCore.Identity;

namespace Identity.Exceptions;

public class InvalidCredentialsException : IdentityException
{
    public InvalidCredentialsException()
        : base("Sorry, that user or/and the password isn't right") { }
}
