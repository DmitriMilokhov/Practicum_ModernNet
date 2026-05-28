using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class UpdateEventTests : EventServiceTestsBase
{
    private readonly EventDto _newEventData = new EventDto
    {
        Title = "Updated Event",
        Description = "Updated Description",
        StartAt = BaseTestStartDate,
        EndAt = BaseTestEndDate,
        TotalSeats = BaseTotalSeats
    };

    [Fact]
    public async Task UpdateEvent_Positive()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var eventToUpdate = dbContext.Events.First();

        //Act
        var action = () => eventService.UpdateEventAsync(eventToUpdate.Id, _newEventData);

        //Assert
        await action.Should().NotThrowAsync();
        await eventRepository.SaveChangesAsync();

        var firstEvent = dbContext.Events.First();
        firstEvent.Should().BeEquivalentTo(_newEventData, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task UpdateEvent_Negative_NotFound()
    {
        //Arrange
        var someId = Guid.NewGuid();
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var expectedExceptionMessage = $"{nameof(Event)} {someId} is not found";

        //Act
        var action = async () => await eventService.UpdateEventAsync(someId, _newEventData);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public async Task UpdateEvent_Negative_ValidationErrors(EventDto eventDto, string expectedExceptionMessage)
    {
        //Arrange
        var someId = Guid.NewGuid();
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        //Act
        var action = async () => await eventService.UpdateEventAsync(someId, eventDto);

        //Assert
        await action.Should().ThrowAsync<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
