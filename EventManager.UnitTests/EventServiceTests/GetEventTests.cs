using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UnitTests.EventServiceTests;

public class GetEventTests : EventServiceTestsBase
{
    [Fact]
    public async Task GetEvent_Positive()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        List<Event> testEvents =
        [
            new Event("First event", "test", BaseTestStartDate, BaseTestEndDate, BaseTotalSeats),
            new Event("Holiday", "holiday", BaseTestStartDate.AddMonths(-4), BaseTestEndDate, BaseTotalSeats),
            new Event("Event 9", "default", BaseTestStartDate.AddYears(-1), BaseTestStartDate.AddYears(1), BaseTotalSeats),
            new Event("WhatIsThis", "Not clear", BaseTestStartDate.AddMonths(-2), BaseTestStartDate.AddMonths(2), BaseTotalSeats),
            new Event("LastEvent", "last", BaseTestStartDate.AddDays(-4), BaseTestEndDate, BaseTotalSeats),
        ];

        await dbContext.Events.AddRangeAsync(testEvents);
        await dbContext.SaveChangesAsync();

        var eventToFind = testEvents.Last();

        //Act
        var result = await eventService.GetEventAsync(eventToFind.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(eventToFind, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetEvent_Negative()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Event)} {randomGuid} is not found";

        //Act
        var action = async () => await eventService.GetEventAsync(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
