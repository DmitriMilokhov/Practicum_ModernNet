using EventManager.DataAccess;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Constants;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventManagerTests.EventServiceTests;

public class GetEventsTests : EventServiceTestsBase
{
    public static IEnumerable<object[]> GetPaginationTestData()
    {
        yield return [new EventFilter(), 2, 10];
        yield return [new EventFilter { Page = 2 }, 2, 1];
        yield return [new EventFilter { PageSize = 5, Page = 2 }, 3, 5];
    }

    [Theory]
    [MemberData(nameof(GetPaginationTestData))]
    public async Task GetEvents_Positive_WithPagination(EventFilter filter, int expectedTotalPages, int expectedItemCounts)
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);
        
        var expectedTotalItems = TestEvents.Count;
        var expectedPageItems = TestEvents
            .OrderByDescending(e => e.StartAt)
            .ThenBy(e => e.Title)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        //Act
        var result = await eventService.GetEventsAsync(filter);
        var actualPageItems = result.Items.ToList();

        //Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(filter.Page);
        result.PageSize.Should().Be(filter.PageSize);
        result.TotalItems.Should().Be(expectedTotalItems);
        result.TotalPages.Should().Be(expectedTotalPages);

        actualPageItems.Should().HaveCount(expectedItemCounts);
        actualPageItems.Should().BeEquivalentTo(expectedPageItems, options => options.ExcludingMissingMembers());
    }

    [Theory]
    [InlineData("Holiday", 1)]
    [InlineData("Event", 9)]
    [InlineData("_", 0)]
    public async Task GetEvents_Positive_TitleFilter(string title, int expectedItemsCount)
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var filter = new EventFilter { Title = title };

        //Act
        var result = await eventService.GetEventsAsync(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetEvents_Positive_StartDateFilter()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var filter = new EventFilter { From = BaseTestStartDate.AddDays(-6), PageSize = 20 };
        var expectedItemsCount = 5;

        //Act
        var result = await eventService.GetEventsAsync(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.StartAt >= filter.From.Value);
    }

    [Fact]
    public async Task GetEvents_Positive_EndDateFilter()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var filter = new EventFilter { To = BaseTestEndDate.AddDays(1), PageSize = 20 };
        var expectedItemsCount = 9;

        //Act
        var result = await eventService.GetEventsAsync(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r => r.EndAt <= filter.To.Value);
    }

    [Fact]
    public async Task GetEvents_Positive_MixFilter()
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedEventsAsync(dbContext);

        var filter = new EventFilter
        {
            Title = "Event",
            From = BaseTestStartDate,
            To = BaseTestEndDate,
            PageSize = 20
        };

        var expectedItemsCount = 1;

        //Act
        var result = await eventService.GetEventsAsync(filter);

        //Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(expectedItemsCount);
        result.Items.Should().OnlyContain(r =>
            r.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase)
            && r.StartAt >= filter.From.Value
            && r.EndAt <= filter.To.Value);
    }


    public static IEnumerable<object[]> GetFilterNegativeTestData()
    {
        yield return [new EventFilter { Page = -2 }, Constants.PageMustBeAboveOrEqualOne];
        yield return [new EventFilter { PageSize = 0 }, Constants.PageSizeMustBeAboveOrEqualOne];
        yield return [new EventFilter { Title = " " }, Constants.TitleFilterWithoutSpacesMsg];
        yield return [new EventFilter { Title = "    " }, Constants.TitleFilterWithoutSpacesMsg];
        yield return [new EventFilter { From = BaseTestEndDate, To = BaseTestStartDate }, Constants.EndDateLaterThanStartMsg];
    }

    [Theory]
    [MemberData(nameof(GetFilterNegativeTestData))]
    public async Task GetEvents_Negative_ValidationErrors(EventFilter filter, string expectedExceptionMessage)
    {
        //Arrange
        using var scope = CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        //Act
        var action = async () => await eventService.GetEventsAsync(filter);

        //Assert
        await action.Should().ThrowAsync<ValidationException>().WithMessage(expectedExceptionMessage);
    }
}
