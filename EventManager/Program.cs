using EventManager.Features.Bookings;
using EventManager.Features.Events;
using EventManager.Infrastructure;
using EventManager.Middleware;

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
builder.Services.AddEventServices();
builder.Services.AddBookingServices();
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
