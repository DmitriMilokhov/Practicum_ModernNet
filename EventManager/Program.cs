using EventManager.DataAccess;
using EventManager.Features.Bookings;
using EventManager.Features.Events;
using EventManager.Infrastructure;
using EventManager.Middleware;
using Microsoft.EntityFrameworkCore;

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

//Db Context
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("Connection string 'Default' not found");
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .LogTo(Console.WriteLine)
    .EnableDetailedErrors());

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();
