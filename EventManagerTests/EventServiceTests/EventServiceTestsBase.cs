using EventManager.DataAccess;
using EventManager.Features.Events;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerTests.EventServiceTests;

public abstract class EventServiceTestsBase : IDisposable
{
    protected static readonly DateTime BaseTestStartDate = new DateTime(2026, 5, 1);
    protected static readonly DateTime BaseTestEndDate = new(2026, 6, 20);
    protected static readonly int BaseTotalSeats = 100;

    protected readonly ServiceProvider ServiceProvider;
    
    protected readonly List<Event> TestEvents =
    [
        new Event("First event", "test", BaseTestStartDate, BaseTestEndDate, BaseTotalSeats),
        new Event("Holiday", "holiday", BaseTestStartDate.AddMonths(-4), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 3", "default", BaseTestStartDate.AddDays(-4), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 4", "default", BaseTestStartDate.AddDays(-5), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 5", "default", BaseTestStartDate.AddDays(-6), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 6", "default", BaseTestStartDate.AddDays(-7), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 7", "default", BaseTestStartDate.AddDays(-8), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 8", "default", BaseTestStartDate.AddYears(-1), BaseTestEndDate, BaseTotalSeats),
        new Event("Event 9", "default", BaseTestStartDate.AddYears(-1), BaseTestStartDate.AddYears(1), BaseTotalSeats),
        new Event("WhatIsThis", "Not clear", BaseTestStartDate.AddMonths(-2), BaseTestStartDate.AddMonths(2), BaseTotalSeats),
        new Event("LastEvent", "last", BaseTestStartDate.AddDays(-4), BaseTestEndDate, BaseTotalSeats),
    ];

    public static IEnumerable<object[]> GetValidationTestData()
    {
        yield return [ new EventDto
            {
                Title = "",
                Description = "I am new",
                StartAt = BaseTestStartDate,
                EndAt = BaseTestEndDate,
                TotalSeats = BaseTotalSeats
            },
            Constants.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = null!,
                Description = "I am new",
                StartAt = BaseTestStartDate,
                EndAt = BaseTestEndDate,
                TotalSeats = BaseTotalSeats
            },
            Constants.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = "New Event",
                Description = "I am new",
                StartAt = BaseTestEndDate,
                EndAt = BaseTestStartDate,
                TotalSeats = BaseTotalSeats
            },
            Constants.EndDateLaterThanStartMsg
        ];

        yield return [ new EventDto
            {
                Title = "New Event",
                Description = "I am new",
                StartAt = BaseTestStartDate,
                EndAt = BaseTestEndDate,
                TotalSeats = 0
            },
            Constants.TotalSeatsAboveZeroMsg
        ];
    }

    protected EventServiceTestsBase()
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventFilterValidator, EventFilterValidator>();
        services.AddScoped<IEventService, EventService>();

        ServiceProvider = services.BuildServiceProvider();
    }

    protected IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }

    protected async Task SeedEventsAsync(AppDbContext dbContext)
    {
        await dbContext.Events.AddRangeAsync(TestEvents);

        await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }


}
