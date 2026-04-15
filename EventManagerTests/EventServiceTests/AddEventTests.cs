using EventManager.Models;
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
        EventRepositoryMock.VerifyNoOtherCalls();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(newEventDto.Title, result.Title);
        Assert.Equal(newEventDto.Description, result.Description);
        Assert.Equal(newEventDto.StartAt, result.StartAt);
        Assert.Equal(newEventDto.EndAt, result.EndAt);
    }    

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public void AddEvent_Negative(EventDto eventDto, string expectedExceptionMessage)
    {
        //Act
        var exception = Record.Exception(() => EventService.AddEvent(eventDto));

        //Assert
        Assert.NotNull(exception);
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }
}
