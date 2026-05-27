using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerTests.EventServiceTests;

public class GetEventTests : EventServiceTestsBase
{
    [Fact]
    public async Task GetEvent_Positive()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var eventToFind = TestEvents.Last();

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
