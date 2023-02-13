using Identity.Dtos;
using Identity.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Identity.Endpoints;

internal static class EndpointsMapper
{
    public static void Map(WebApplication app)
    {
        var routesGroup = app.MapGroup("/users");

        routesGroup.MapPost("",
            async (UserSignUpInput input, IIdentityService identityService) =>
            {
                await identityService.SignUp(input);
                return TypedResults.Ok();
            })
            .WithOpenApi()
            .WithTags("Auth")
            .WithName("AuthSignUp")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        routesGroup.MapPost("/login",
            async (UserLoginInput input, IIdentityService identityService) =>
            {
                var output = await identityService.Login(input);
                return TypedResults.Ok(output);
            })
            .WithOpenApi()
            .WithTags("Auth")
            .WithName("AuthLogin")
            .Produces<UserLoginOutput>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        app.MapGet("/test",
            () => Results.Ok(new TestOutput()))
            .Produces<TestOutput>(StatusCodes.Status200OK)
            .WithOpenApi()
            .WithTags("Test (Private)");
    }

    public record TestOutput(bool? Success = true);
}
