using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Yarp.Demo.ExternalApi;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add proxy to InternalApi
builder.Services.AddHttpForwarder();

var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
});
var transformer = new Utf8VowelTransformer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("api/{clientId}/{marketId:int}/hello", (string clientId, int marketId) => $"[{clientId}:{marketId}] ExternalApi: Hello!");
app.MapPost("api/{clientId}/{marketId:int}/echo", (string clientId, int marketId, [FromBody] string text) => $"[{clientId}:{marketId}] ExternalApi: {text}");

app.UseRouting();

// map endpoints to InternalApi
app.UseEndpoints(endpoints =>
{
    endpoints.Map("/api/{clientId}/{marketId:int}/internal/{**catch-all}", async ctx =>
    {
        
        var forwarder = ctx.RequestServices.GetRequiredService<IHttpForwarder>();
        var error = await forwarder.SendAsync(ctx, "https://localhost:5002", httpClient, new ForwarderRequestConfig(),
            transformer);
        // Check if the proxy operation was successful
        if (error != ForwarderError.None)
        {
            var errorFeature = ctx.Features.Get<IForwarderErrorFeature>();
            var exception = errorFeature?.Exception;
            Console.WriteLine(exception);
        }
    });
});

app.Run();
