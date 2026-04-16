using EventManager.Infrastructure.Exceptions;
using Moq;

namespace EventManagerTests.EventServiceTests;

public class GetEventTests : EventServiceTestsBase
{
    [Fact]
    public void GetEvent_Positive()
    {
        //Arrange
        var eventToFind = TestEvents.Last();
        EventRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>())).Returns(eventToFind);

        //Act
        var result = EventService.GetEvent(eventToFind.Id);

        //Assert
        EventRepositoryMock.Verify(r => r.Get(It.IsAny<Guid>()), Times.Once());

        Assert.NotNull(result);
        Assert.Equal(eventToFind.Id, result.Id);
    }

    [Fact]
    public void GetEvent_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"Event {randomGuid} is not found";

        EventRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>())).Throws(new EventNotFoundException(randomGuid));

        //Act
        var exception = Record.Exception(() => EventService.GetEvent(randomGuid));

        //Assert
        Assert.NotNull(exception);
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }
}
