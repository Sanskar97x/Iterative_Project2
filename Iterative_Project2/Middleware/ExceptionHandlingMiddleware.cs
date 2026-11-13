using Iterative_Project2.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Iterative_Project2.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        // Created once, not on every error
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
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
            // Client closed the connection: not an error, nobody is listening
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request was cancelled by the client: {Path}", context.Request.Path);
            }
            // Response already started: headers/status can't change, so keep the original stack trace
            catch (Exception ex) when (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception after the response had started: {Path}", context.Request.Path);
                throw;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (status, title, safeToShow) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", true),
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad request", true),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", true),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", true),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", true),
                DbUpdateException db when IsUniqueViolation(db)
                                      => (StatusCodes.Status409Conflict, "Duplicate value", false),
                _ => (StatusCodes.Status500InternalServerError,
                                          "An unexpected error occurred", false)
            };

            // Expected errors are Warnings, only real bugs are Errors
            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("Request failed with {Status}: {Message}", status, ex.Message);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = safeToShow
                    ? ex.Message
                    : status == StatusCodes.Status409Conflict
                        ? "A record with the same value already exists."
                        : _env.IsDevelopment() ? ex.Message : null   // real message only in Development
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            // No Response.Clear(): HasStarted is false here, so there is no body to discard,
            // and Clear() would also remove headers set earlier (such as CORS).
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }

        // SQL Server: 2601 / 2627 = unique index / unique constraint violation
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? string.Empty;
            return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase);
        }
    }
}