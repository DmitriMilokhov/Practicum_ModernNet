using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class GetEventTests : EventServiceTestsBase
{
    [Fact]
    public void GetEvent_Positive()
    {
        //Arrange
        var eventToFind = TestEvents.Last();
        EventRepositoryMock.Setup(r => r.Get(eventToFind.Id)).Returns(eventToFind);

        //Act
        var result = EventService.GetEvent(eventToFind.Id);

        //Assert
        EventRepositoryMock.Verify(r => r.Get(eventToFind.Id), Times.Once());

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(eventToFind, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public void GetEvent_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"Event {randomGuid} is not found";

        EventRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>())).Throws(new EventNotFoundException(randomGuid));

        //Act
        var action = () => EventService.GetEvent(randomGuid);

        //Assert
        action.Should().Throw<EventNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
