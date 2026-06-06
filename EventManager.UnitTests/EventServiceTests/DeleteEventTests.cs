using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UnitTests.EventServiceTests;

public class DeleteEventTests : EventServiceTestsBase
{
    [Fact]
    public async Task DeleteEvent_Positive()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<Event> testEvents =
        [
            new Event("First event", "test", BaseTestStartDate, BaseTestEndDate, BaseTotalSeats),
            new Event("Holiday", "holiday", BaseTestStartDate.AddMonths(-4), BaseTestEndDate, BaseTotalSeats),
            new Event("WhatIsThis", "Not clear", BaseTestStartDate.AddMonths(-2), BaseTestStartDate.AddMonths(2), BaseTotalSeats),
            new Event("LastEvent", "last", BaseTestStartDate.AddDays(-4), BaseTestEndDate, BaseTotalSeats),
        ];

        await dbContext.Events.AddRangeAsync(testEvents);
        await dbContext.SaveChangesAsync();

        var initialCount = dbContext.Events.Count();
        var eventToDelete = dbContext.Events.Last();

        //Act
        var action = () => eventService.DeleteEventAsync(eventToDelete.Id);
        await eventRepository.SaveChangesAsync();

        //Assert
        await action.Should().NotThrowAsync();
        dbContext.Events.Count().Should().Be(initialCount-1);
    }

    [Fact]
    public async Task DeleteEvent_Negative_NotFound()
    {
        //Arrange
        var someId = Guid.NewGuid();
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var expectedExceptionMessage = $"{nameof(Event)} {someId} is not found";

        //Act
        var action = () => eventService.DeleteEventAsync(someId);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
