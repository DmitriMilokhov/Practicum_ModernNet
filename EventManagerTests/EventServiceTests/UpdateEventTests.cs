using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class UpdateEventTests : EventServiceTestsBase
{
    private readonly Guid _eventIdToUpdate;
    private readonly EventDto _newEventData = new EventDto
    {
        Title = "Updated Event",
        Description = "Updated Description",
        StartAt = BaseTestStartDate,
        EndAt = BaseTestEndDate,
    };

    public UpdateEventTests()
    {
        _eventIdToUpdate = TestEvents.First().Id;
    }
    [Fact]
    public async Task UpdateEvent_Positive()
    {
        //Act
        await EventService.UpdateEventAsync(_eventIdToUpdate, _newEventData);

        //Assert
        EventRepositoryMock.Verify(r => r.UpdateAsync(_eventIdToUpdate, It.Is<Event>(e =>
            e.Title == _newEventData.Title &&
            e.Description == _newEventData.Description &&
            e.StartAt == _newEventData.StartAt &&
            e.EndAt == _newEventData.EndAt),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task UpdateEvent_Negative_NotFound()
    {
        //Arrange
        var expectedExceptionMessage = $"Event {_eventIdToUpdate} is not found";

        EventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), 
                                                                     It.IsAny<Event>(), 
                                                                     It.IsAny<CancellationToken>()))
                           .Throws(new EventNotFoundException(_eventIdToUpdate));

        //Act
        var action = async () => await EventService.UpdateEventAsync(_eventIdToUpdate, _newEventData);

        //Assert
        await action.Should().ThrowAsync<EventNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public async Task UpdateEvent_Negative_ValidationErrors(EventDto eventDto, string expectedExceptionMessage)
    {
        //Act
        var action = async () => await EventService.UpdateEventAsync(_eventIdToUpdate, eventDto);

        //Assert
        await action.Should().ThrowAsync<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
