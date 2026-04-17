using EventManager.Infrastructure;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;
using EventManager.Models.Filters;
using EventManager.Services;
using Moq;

namespace EventManagerTests.EventServiceTests;

public abstract class EventServiceTestsBase
{
    protected static readonly DateTime BaseTestStartDate = new DateTime(2026, 5, 1);
    protected static readonly DateTime BaseTestEndDate = new(2026, 6, 20);

    protected readonly Mock<IEventRepository> EventRepositoryMock = new();
    protected readonly EventService EventService;
    
    protected readonly List<Event> TestEvents =
    [
        new Event(Guid.NewGuid(), "First event", "test", BaseTestStartDate, BaseTestEndDate),
        new Event(Guid.NewGuid(), "Holiday", "holiday", BaseTestStartDate.AddMonths(-4), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 3", "default", BaseTestStartDate.AddDays(-4), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 4", "default", BaseTestStartDate.AddDays(-5), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 5", "default", BaseTestStartDate.AddDays(-6), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 6", "default", BaseTestStartDate.AddDays(-7), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 7", "default", BaseTestStartDate.AddDays(-8), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 8", "default", BaseTestStartDate.AddYears(-1), BaseTestEndDate),
        new Event(Guid.NewGuid(), "Event 9", "default", BaseTestStartDate.AddYears(-1), BaseTestStartDate.AddYears(1)),
        new Event(Guid.NewGuid(), "WhatIsThis", "Not clear", BaseTestStartDate.AddMonths(-2), BaseTestStartDate.AddMonths(2)),
        new Event(Guid.NewGuid(), "LastEvent", "last", BaseTestStartDate.AddDays(-4), BaseTestEndDate),
    ];

    protected EventServiceTestsBase()
    {
        EventService = new EventService(EventRepositoryMock.Object, new EventFilterValidator());
    }

    public static IEnumerable<object[]> GetValidationTestData()
    {
        yield return [ new EventDto
            {
                Title = "",
                Description = "I am new",
                StartAt = BaseTestStartDate,
                EndAt = BaseTestEndDate
            },
            ValidationMessages.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = null!,
                Description = "I am new",
                StartAt = BaseTestStartDate,
                EndAt = BaseTestEndDate
            },
            ValidationMessages.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = "New Event",
                Description = "I am new",
                StartAt = BaseTestEndDate,
                EndAt = BaseTestStartDate
            },
            ValidationMessages.EndDateLaterThanStartMsg
        ];
    }
}
