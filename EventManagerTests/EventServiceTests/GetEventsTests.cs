using EventManager.Models.Filters;
using Moq;

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

        //Act
        var result = EventService.GetEvents(filter);

        //Assert
        EventRepositoryMock.Verify(r => r.GetAll(), Times.Once());
        EventRepositoryMock.VerifyNoOtherCalls();

        Assert.NotNull(result);

        Assert.Equal(filter.Page, result.Page);
        Assert.Equal(filter.PageSize, result.PageSize);
        Assert.Equal(expectedTotalItems, result.TotalItems);
        Assert.Equal(expectedTotalPages, result.TotalPages);
        Assert.Equal(expectedItemCounts, result.Items.Count());

        Assert.All(result.Items, r => Assert.True(TestEvents.Any(e => e.Id == r.Id)));
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
        Assert.NotNull(result);
        Assert.Equal(expectedItemsCount, result.TotalItems);
        Assert.All(result.Items, r => Assert.True(r.Title.Contains(title, StringComparison.OrdinalIgnoreCase)));
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
        Assert.NotNull(result);
        Assert.Equal(expectedItemsCount, result.TotalItems);
        Assert.All(result.Items, r => Assert.True(r.StartAt >= filter.From.Value));
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
        Assert.NotNull(result);
        Assert.Equal(expectedItemsCount, result.TotalItems);
        Assert.All(result.Items, r => Assert.True(r.EndAt <= filter.To.Value));
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
        Assert.NotNull(result);
        Assert.Equal(expectedItemsCount, result.TotalItems);
        Assert.All(result.Items, r => Assert.True(r.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase)
            && r.StartAt >= filter.From.Value && r.EndAt <= filter.To.Value));
    }
}
