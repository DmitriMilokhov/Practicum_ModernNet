using EventManager.Models;
using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class AddEventTests : EventServiceTestsBase
{
    [Fact]
    public void AddEvent_Positive()
    {
        //Arrange
        var newEventDto = new EventDto()
        {
            Title = "Some new event",
            Description = "I am new",
            StartAt = DateTime.Now.AddDays(-5),
            EndAt = DateTime.Now.AddDays(5)
        };

        //Act
        var result = EventService.AddEvent(newEventDto);

        //Assert
        EventRepositoryMock.Verify(r => r.Add(It.Is<Event>(e => e.Title == newEventDto.Title)),
            Times.Once());

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Should().BeEquivalentTo(newEventDto);
    }    

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public void AddEvent_Negative(EventDto eventDto, string expectedExceptionMessage)
    {
        //Act
        var action = () => EventService.AddEvent(eventDto);

        //Assert
        action.Should().Throw<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
