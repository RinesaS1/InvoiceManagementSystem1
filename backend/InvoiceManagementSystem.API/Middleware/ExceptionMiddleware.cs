using System.Net;
using System.Text.Json;
using InvoiceManagementSystem.API.DTOs;

namespace InvoiceManagementSystem.API.Middleware;

/// <summary>
/// ExceptionMiddleware — Kap çdo gabim të patrajtuar në aplikacion.
/// Në vend të mesazhit të gabimit standard të .NET, kthen një JSON të pastër
/// me formatin ApiResponse, pa ekspozuar detaje teknike te klienti.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gabim i patrajtuar: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var message = _env.IsDevelopment()
            ? ex.Message
            : "Ndodhi një gabim i brendshëm. Ju lutemi provoni përsëri.";

        var response = ApiResponse.Fail(message);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
