using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using StrategyPattern.Api.Application.Features.Users.Commands.AddUser;
using StrategyPattern.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InitializeInfrastructre(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();


app.MapPost("/AddUser", AddUser)
    .WithName("AddUser");

static async Task<IResult> AddUser(
    [FromServices] ISender sender,
    [FromBody] AddUserRequest request)
{
    var command = new AddUserCommand(request.FirstName, request.LastName);
    var result = await sender.Send(command);
    if (result.IsSuccess)
        return Results.Ok(result.Value);
    
    return  Results.BadRequest(result.Error);
}

app.Run();