using EventManager.Models;
using EventManager.Models.Filters;
using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class GetEventsTests : EventServiceTestsBase
{
    public GetEventsTests()
    {
        EventRepositoryMock.Setup(mock => mock.GetAll()).Returns(TestEvents);
    }

    public static IEnumerable<object[]> GetPaginationTestData()
    {
        yield return [new EventFilter(), 2, 10];
        yield return [new EventFilter { Page = 2 }, 2, 1];
        yield return [new EventFilter { PageSize = 5, Page = 2 }, 3, 5];
    }

    [Theory]
    [MemberData(nameof(GetPaginationTestData))]
    public void GetEvents_Positive_WithPagination(EventFilter filter, int expectedTotalPages, int expectedItemCounts)
    {
        //Arrange
        var expectedTotalItems = TestEvents.Count;
        var expectedPageItems = TestEvents
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        //Act
        var result = EventService.GetEvents(filter);
        var actualPageItems = result.Items.ToList();

        //Assert
        EventRepositoryMock.Verify(r => r.GetAll(), Times.Once());

        result.Should().NotBeNull();
        result.Page.Should().Be(filter.Page);
        result.PageSize.Should().Be(filter.PageSize);
        result.TotalItems.Should().Be(expectedTotalItems);
        result.TotalPages.Should().Be(expectedTotalPages);

        actualPageItems.Should().HaveCount(expectedItemCounts);
        actualPageItems.Should().BeEquivalentTo(expectedPageItems, options => options.ComparingByMembers<FullEventDto>());
    }

    [Theory]
    [InlineData("Holiday", 1)]
    [InlineData("Event", 9)]
    public void GetEvents_Positive_TitleFilter(string title, int expectedItemsCount)
    {
        //Arrange
        var filter = new EventFilter { Title = title };

        //Act
        var result = EventService.GetEvents(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetEvents_Positive_StartDateFilter()
    {
        //Arrange
        var filter = new EventFilter { From = DateTime.Now.AddDays(-6), PageSize = 20 };
        var expectedItemsCount = 3;

        //Act
        var result = EventService.GetEvents(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.StartAt >= filter.From.Value);
    }

    [Fact]
    public void GetEvents_Positive_EndDateFilter()
    {
        //Arrange
        var filter = new EventFilter { To = DateTime.Now.AddDays(1), PageSize = 20 };
        var expectedItemsCount = 9;

        //Act
        var result = EventService.GetEvents(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.EndAt <= filter.To.Value);
    }

    [Fact]
    public void GetEvents_Positive_MixFilter()
    {
        //Arrange
        var filter = new EventFilter
        {
            Title = "Event",
            From = DateTime.Now.AddDays(-5),
            To = DateTime.Now.AddDays(1),
            PageSize = 20
        };

        var expectedItemsCount = 2;

        //Act
        var result = EventService.GetEvents(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => 
            r.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase) 
            && r.StartAt >= filter.From.Value 
            && r.EndAt <= filter.To.Value);
    }


    public static IEnumerable<object[]> GetPaginationNegativeTestData()
    {
        yield return [new EventFilter { Page = -2 }, "Page must be greater than or equal to 1"];
        yield return [new EventFilter { PageSize = 0 }, "PageSize must be greater than or equal to 1"];
    }

    [Theory]
    [MemberData(nameof(GetPaginationNegativeTestData))]
    public void GetEvents_Negative_ValidationErrors(EventFilter filter, string expectedExceptionMessage)
    {
        //Act
        var action = () => EventService.GetEvents(filter);

        //Assert
        action.Should().Throw<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
