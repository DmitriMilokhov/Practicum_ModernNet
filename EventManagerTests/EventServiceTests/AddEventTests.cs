using EventManager.Models;
using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class AddEventTests : EventServiceTestsBase
{
    [Fact]
    public async Task AddEvent_Positive()
    {
        //Arrange
        var newEventDto = new EventDto()
        {
            Title = "Some new event",
            Description = "I am new",
            StartAt = BaseTestStartDate,
            EndAt = BaseTestEndDate
        };

        //Act
        var result = await EventService.AddEventAsync(newEventDto);

        //Assert
        EventRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Event>(e => e.Title == newEventDto.Title),
            It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Should().BeEquivalentTo(newEventDto);
    }    

    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public async Task AddEvent_Negative(EventDto eventDto, string expectedExceptionMessage)
    {
        //Act
        var action = async () => await EventService.AddEventAsync(eventDto);

        //Assert
        await action.Should().ThrowAsync<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
