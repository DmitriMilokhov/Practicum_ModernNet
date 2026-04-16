using EventManager.Infrastructure;
using EventManager.Interfaces.IRepositories;
using EventManager.Models;
using EventManager.Models.Filters;
using EventManager.Services;
using Moq;

namespace EventManagerTests.EventServiceTests;

public abstract class EventServiceTestsBase
{
    protected readonly Mock<IEventRepository> EventRepositoryMock = new();
    protected readonly EventService EventService;

    protected readonly List<Event> TestEvents =
    [
        new Event(Guid.NewGuid(), "First event", "test", DateTime.Now.AddMonths(-3), DateTime.Now),
        new Event(Guid.NewGuid(), "Holiday", "holiday", DateTime.Now.AddMonths(-4), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 3", "default", DateTime.Now.AddDays(-4), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 4", "default", DateTime.Now.AddDays(-5), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 5", "default", DateTime.Now.AddDays(-6), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 6", "default", DateTime.Now.AddDays(-7), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 7", "default", DateTime.Now.AddDays(-8), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 8", "default", DateTime.Now.AddYears(-1), DateTime.Now),
        new Event(Guid.NewGuid(), "Event 9", "default", DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1)),
        new Event(Guid.NewGuid(), "WhatIsThis", "Not clear", DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2)),
        new Event(Guid.NewGuid(), "LastEvent", "last", DateTime.Now.AddDays(-4), DateTime.Now),
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
                StartAt = DateTime.Now.AddDays(-5),
                EndAt = DateTime.Now.AddDays(5)
            },
            ValidationMessages.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = null!,
                Description = "I am new",
                StartAt = DateTime.Now.AddDays(-5),
                EndAt = DateTime.Now.AddDays(5)
            },
            ValidationMessages.TitleIsRequiredMsg
        ];

        yield return [ new EventDto
            {
                Title = "New Event",
                Description = "I am new",
                StartAt = DateTime.Now.AddDays(5),
                EndAt = DateTime.Now.AddDays(-5)
            },
            ValidationMessages.EndDateLaterThanStartMsg
        ];
    }
}
