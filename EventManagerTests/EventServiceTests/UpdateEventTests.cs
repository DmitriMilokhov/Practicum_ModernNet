using EventManager.Infrastructure.Exceptions;
using EventManager.Models;
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
    public void UpdateEvent_Positive()
    {
        //Act
        EventService.UpdateEvent(_eventIdToUpdate, _newEventData);

        //Assert
        EventRepositoryMock.Verify(r => r.Update(_eventIdToUpdate, It.Is<Event>(e =>
            e.Title == _newEventData.Title &&
            e.Description == _newEventData.Description &&
            e.StartAt == _newEventData.StartAt &&
            e.EndAt == _newEventData.EndAt)), Times.Once());
    }

    [Fact]
    public void UpdateEvent_Negative_NotFound()
    {
        //Arrange
        var expectedExceptionMessage = $"Event {_eventIdToUpdate} is not found";

        EventRepositoryMock.Setup(r => r.Update(It.IsAny<Guid>(), It.IsAny<Event>()))
                           .Throws(new EventNotFoundException(_eventIdToUpdate));

        //Act
        var action = () => EventService.UpdateEvent(_eventIdToUpdate, _newEventData);

        //Assert
        action.Should().Throw<EventNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public void UpdateEvent_Negative_ValidationErrors(EventDto eventDto, string expectedExceptionMessage)
    {
        //Act
        var action = () => EventService.UpdateEvent(_eventIdToUpdate, eventDto);

        //Assert
        action.Should().Throw<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
