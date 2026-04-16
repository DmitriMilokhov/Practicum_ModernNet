using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;

namespace EventManagerTests.EventServiceTests;

public class DeleteEventTests : EventServiceTestsBase
{
    private readonly Guid _eventIdToUpdate;

    public DeleteEventTests()
    {
        _eventIdToUpdate = TestEvents.First().Id;
    }

    [Fact]
    public void DeleteEvent_Positive()
    {
        //Act
        EventService.DeleteEvent(_eventIdToUpdate);

        //Assert
        EventRepositoryMock.Verify(r => r.Delete(_eventIdToUpdate));
    }

    [Fact]
    public void DeleteEvent_Negative_NotFound()
    {
        //Arrange
        var expectedExceptionMessage = $"Event {_eventIdToUpdate} is not found";

        EventRepositoryMock.Setup(r => r.Delete(It.IsAny<Guid>()))
                           .Throws(new EventNotFoundException(_eventIdToUpdate));

        //Act
        var action = () => EventService.DeleteEvent(_eventIdToUpdate);

        //Assert
        action.Should().Throw<EventNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
