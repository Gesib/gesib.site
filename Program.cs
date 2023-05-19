using ASPMicroService.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterApplicationServices();

var app = builder.Build();

app.ConfigureMiddleware(app.Environment);

app.ConfigureEndpoints();

app.Run();