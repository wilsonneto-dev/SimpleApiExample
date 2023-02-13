using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace Identity.Exceptions;

public class IdentityException : Exception
{
    public ReadOnlyCollection<string>? Details { get; private set; }
    public IdentityException(string exceptionMessage, ReadOnlyCollection<string>? details = null) : base(exceptionMessage)
        => Details = details;

    public IdentityException(string? message, IEnumerable<IdentityError> errors) : base(message)
        => Details = errors.Select(err => $"{err.Code} {err.Description}").ToList().AsReadOnly();

}
