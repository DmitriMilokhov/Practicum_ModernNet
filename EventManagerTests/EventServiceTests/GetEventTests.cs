using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class GetEventTests : EventServiceTestsBase
{
    [Fact]
    public async Task GetEvent_Positive()
    {
        //Arrange
        var eventToFind = TestEvents.Last();
        EventRepositoryMock.Setup(r => r.GetAsync(eventToFind.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventToFind);

        //Act
        var result = await EventService.GetEventAsync(eventToFind.Id);

        //Assert
        EventRepositoryMock.Verify(r => r.GetAsync(eventToFind.Id, It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(eventToFind, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetEvent_Negative()
    {
        //Arrange
        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Event)} {randomGuid} is not found";

        EventRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(nameof(Event), randomGuid));

        //Act
        var action = async () => await EventService.GetEventAsync(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }
}
