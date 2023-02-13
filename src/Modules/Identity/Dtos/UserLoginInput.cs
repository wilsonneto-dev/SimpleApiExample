using System.ComponentModel.DataAnnotations;

namespace Identity.Dtos;

public record UserLoginInput(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password
);