using ManteqTask.Application;
using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Application.Utilities;
using ManteqTask.Domain;
using ManteqTask.Infrastructure;
using ManteqTask.Presentation.API;
// using ManteqTask.Presentation.API.Configurations;
using ManteqTask.Presentation.API.Middlewares;
using ManteqTask.Presentation.API.Models;
using ManteqTask.Presentation.API.Routes.Authentication;
using ManteqTask.Presentation.API.Routes.Requests;
using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Enums;

using RefreshToken = ManteqTask.Presentation.API.Routes.Authentication.RefreshToken;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetRequiredSetting("ConnectionStrings:DbConnectionString"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddSwaggerAuth();

builder.Services.AddDomain()
                .AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddPresentation(builder.Configuration);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Map health check endpoint
app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet Template API v1");
        c.DocumentTitle = "Dotnet Template API";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.MapGet("/", () => new
{
    message = "Welcome to My API",
    version = "1.0.0",
    links = new
    {
        self = "/",
        docs = "/swagger",
        health = "/health"
    }
}).WithTags("Home");

#region Authentication

app.MapPost("/users/register", RegisterUser.RegisterRoute)
    .WithTags("Authentication")
   .Produces<ApiResponse<RegisterUserCommandResult>>(StatusCodes.Status201Created, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces<ApiResponse<RegisterUserCommandResult>>(StatusCodes.Status409Conflict, "application/json")
   .Accepts<RegisterUserCommand>("application/json");

app.MapPost("/users/login", Login.RegisterRoute)
    .WithTags("Authentication")
   .Produces<ApiResponse<LoginCommandResult>>(StatusCodes.Status200OK, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces<ApiResponse<LoginCommandResult>>(StatusCodes.Status401Unauthorized, "application/json")
   .Accepts<LoginCommand>("application/json");

app.MapPost("/users/refresh-token", RefreshToken.RegisterRoute)
    .WithTags("Authentication")
    .RequireAuthorization()
   .Produces<ApiResponse<RefreshTokenCommandResult>>(StatusCodes.Status200OK, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces<ApiResponse<RefreshTokenCommandResult>>(StatusCodes.Status401Unauthorized, "application/json")
   .Accepts<RefreshTokenCommand>("application/json");

app.MapPost("/users/logout", Logout.RegisterRoute)
    .WithTags("Authentication")
    .RequireAuthorization()
   .Produces<ApiResponse<LogoutCommandResult>>(StatusCodes.Status200OK, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces<ApiResponse<LogoutCommandResult>>(StatusCodes.Status401Unauthorized, "application/json")
   .Accepts<LogoutCommand>("application/json");

#endregion

#region Requests

app.MapGet("/api/requests", ListRequests.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization()
   .Produces<ApiResponse<PaginationResult<RequestResult>>>(StatusCodes.Status200OK, "application/json")
   .Produces(StatusCodes.Status401Unauthorized);

app.MapPost("/api/requests", CreateRequest.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization(PermissionConstants.Requests.Create)
   .Produces<ApiResponse<RequestResult>>(StatusCodes.Status201Created, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces(StatusCodes.Status401Unauthorized)
   .Produces(StatusCodes.Status403Forbidden)
   .Accepts<CreateRequestCommand>("application/json");

app.MapPut("/api/requests/{id:guid}", EditRequest.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization(PermissionConstants.Requests.Edit)
   .Produces<ApiResponse<RequestResult>>(StatusCodes.Status200OK, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces(StatusCodes.Status403Forbidden)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status409Conflict)
   .Accepts<EditRequestCommand>("application/json");

app.MapPost("/api/requests/{id:guid}/submit", SubmitRequest.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization(PermissionConstants.Requests.Submit)
   .Produces<ApiResponse<RequestResult>>(StatusCodes.Status200OK, "application/json")
   .Produces(StatusCodes.Status403Forbidden)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status409Conflict);

app.MapPost("/api/requests/{id:guid}/approve", ApproveRequest.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization(PermissionConstants.Requests.Approve)
   .Produces<ApiResponse<RequestResult>>(StatusCodes.Status200OK, "application/json")
   .Produces(StatusCodes.Status403Forbidden)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status409Conflict)
   .Accepts<ApproveRequestCommand>("application/json");

app.MapPost("/api/requests/{id:guid}/reject", RejectRequest.RegisterRoute)
    .WithTags("Requests")
    .RequireAuthorization(PermissionConstants.Requests.Reject)
   .Produces<ApiResponse<RequestResult>>(StatusCodes.Status200OK, "application/json")
   .Produces<ApiResponse<IEnumerable<string>>>(StatusCodes.Status400BadRequest, "application/json")
   .Produces(StatusCodes.Status403Forbidden)
   .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status409Conflict)
   .Accepts<RejectRequestCommand>("application/json");

#endregion

app.Run();

/// <summary>
/// The main entry point for the ManteqTask.Presentation.API.
/// This partial class allows the program to be extended with additional methods or configurations.
/// </summary>
public partial class Program { }
