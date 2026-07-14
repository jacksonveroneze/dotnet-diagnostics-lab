using JacksonVeroneze.NET.GRPCServer.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configure();

var app = builder.Build();

app.Configure();

await app.RunAsync();
