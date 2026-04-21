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
    public async Task DeleteEvent_Positive()
    {
        //Act
        await EventService.DeleteEventAsync(_eventIdToUpdate);

        //Assert
        EventRepositoryMock.Verify(r => r.DeleteAsync(_eventIdToUpdate, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteEvent_Negative_NotFound()
    {
        //Arrange
        var expectedExceptionMessage = $"Event {_eventIdToUpdate} is not found";

        EventRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                           .Throws(new EventNotFoundException(_eventIdToUpdate));

        //Act
        var action = () => EventService.DeleteEventAsync(_eventIdToUpdate);

        //Assert
        await action.Should().ThrowAsync<EventNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
