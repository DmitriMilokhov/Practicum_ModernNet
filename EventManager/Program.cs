using EventManager.Infrastructure;
using EventManager.Middleware;
using EventManager.Services;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

//before Build
if(isDevelopment)
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

//services
builder.Services.AddAppServices();
builder.Services.AddInfrastructure();

builder.Logging.AddConsole();

//after build
var app = builder.Build();

app.UseExceptionHandling();

if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers();

app.Run();
