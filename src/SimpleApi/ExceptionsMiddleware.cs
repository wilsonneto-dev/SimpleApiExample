using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SimpleApi;

class ExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            await HandleException(ex, context);
        }
    }

    private async Task HandleException(Exception ex, HttpContext context)
    {
        _logger.LogWarning("exception happened");
        ProblemDetails problem = ex switch
        {
            BadHttpRequestException => new()
            {
                Title = "Bad request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = ex.GetType().Name
            },
            _ => new()
            {
                Title = "Unexpected error happened",
                Type = ex.GetType().Name,
                Detail = ex.Message
            }
        };
        if(_env.IsDevelopment())
            problem.Extensions.Add("Trace", ex.StackTrace);
        _logger.LogWarning("problem: {@problem}", JsonSerializer.Serialize(problem));

        context.Response.StatusCode = (int)(problem.Status ?? StatusCodes.Status500InternalServerError);
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

public static class ExceptionsMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionsMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionsMiddleware>();
}