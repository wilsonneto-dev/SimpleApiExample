using System.ComponentModel.DataAnnotations;

namespace Identity.Dtos;

public record UserSignUpInput(
    [property: Required] string UserName,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);
