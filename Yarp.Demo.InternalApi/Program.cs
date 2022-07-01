using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("api/{clientId}/{marketId:int}/hello", (string clientId, int marketId) => $"[{clientId}:{marketId}] InternalApi: Hello!");
app.MapPost("api/{clientId}/{marketId:int}/echo", (string clientId, int marketId, [FromBody] string text) => $"[{clientId}:{marketId}] InternalApi: {text}");

app.Run();
