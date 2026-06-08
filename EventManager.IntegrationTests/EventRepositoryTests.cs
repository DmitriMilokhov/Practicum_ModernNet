using EventManager.Features.Bookings.Model;
using EventManager.Features.Events;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using EventManager.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

[Collection("Postgres")]
public class EventRepositoryTests(PostgreSqlFixture fixture) 
{
    private readonly DateTime _baseTestStartDate = new(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _baseTestEndDate = new(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc);
    private readonly int _baseTotalSeats = 100;

    #region Create

    [Fact]
    public async Task AddEvent_Positive()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0,0,0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        var repository = new EventRepository(context);

        // Act
        await repository.AddAsync(eventModel);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = fixture.CreateContext();
        var savedEvent = await verifyContext.Events.FirstOrDefaultAsync(b => b.Title == "Test Event");

        savedEvent.Should().NotBeNull();
        savedEvent.Id.Should().NotBe(Guid.Empty);
        savedEvent.Should().BeEquivalentTo(eventModel);
        savedEvent.AvailableSeats.Should().Be(eventModel.TotalSeats);
    }

    #endregion

    #region ReadSingle

    [Fact]
    public async Task GetEvent_Positive()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        // Act
        var result = await repository.GetAsync(eventModel.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(eventModel);
    }

    [Fact]
    public async Task GetEvent_Negative_NotFound()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Event)} {randomGuid} is not found";

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        // Act
        var action = async () => await repository.GetAsync(randomGuid);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    #endregion

    #region ReadMultiple
    public static IEnumerable<object[]> GetPaginationTestData()
    {
        yield return [new EventFilter(), 10];
        yield return [new EventFilter { Page = 2 }, 1];
        yield return [new EventFilter { PageSize = 5, Page = 2 }, 5];
    }

    [Theory]
    [MemberData(nameof(GetPaginationTestData))]
    public async Task GetEvents_Positive_WithPagination(EventFilter filter, int expectedItemCounts)
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        var expectedTotalItems = testEvents.Count;
        var expectedPageItems = testEvents
            .OrderByDescending(e => e.StartAt)
            .ThenBy(e => e.Title)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        var (result, totalItems) = await repository.GetPagedAsync(filter);

        //Assert
        totalItems.Should().Be(expectedTotalItems);

        result.Should().NotBeNull();
        result.Should().HaveCount(expectedItemCounts);
        result.Should().BeEquivalentTo(expectedPageItems);
    }

    [Theory]
    [InlineData("Holiday", 1)]
    [InlineData("Event", 9)]
    [InlineData("_", 0)]
    public async Task GetEvents_Positive_TitleFilter(string title, int expectedItemsCount)
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);
        var filter = new EventFilter { Title = title };

        //Act
        var (result, totalItems) = await repository.GetPagedAsync(filter);

        //Assert
        result.Should().NotBeNull();
        totalItems.Should().Be(expectedItemsCount);
        result.Should().OnlyContain(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetEvents_Positive_StartDateFilter()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);
        var filter = new EventFilter { From = _baseTestStartDate.AddDays(-6), PageSize = 20 };
        var expectedItemsCount = 5;

        //Act
        var (result, totalItems) = await repository.GetPagedAsync(filter);

        //Assert
        result.Should().NotBeNull();
        totalItems.Should().Be(expectedItemsCount);
        result.Should().OnlyContain(r => r.StartAt >= filter.From.Value);
    }

    [Fact]
    public async Task GetEvents_Positive_EndDateFilter()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        var filter = new EventFilter { To = _baseTestEndDate.AddDays(1), PageSize = 20 };
        var expectedItemsCount = 9;

        //Act
        var (result, totalItems) = await repository.GetPagedAsync(filter);

        //Assert
        result.Should().NotBeNull();
        totalItems.Should().Be(expectedItemsCount);
        result.Should().OnlyContain(r => r.EndAt <= filter.To.Value);
    }

    [Fact]
    public async Task GetEvents_Positive_MixFilter()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        var filter = new EventFilter
        {
            Title = "Event",
            From = _baseTestStartDate,
            To = _baseTestEndDate,
            PageSize = 20
        };

        var expectedItemsCount = 1;

        //Act
        var (result, totalItems) = await repository.GetPagedAsync(filter);

        //Assert
        result.Should().NotBeNull();
        totalItems.Should().Be(expectedItemsCount);
        result.Should().OnlyContain(r =>
            r.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase)
            && r.StartAt >= filter.From.Value
            && r.EndAt <= filter.To.Value);
    }

    [Fact]
    public async Task GetEvents_Positive_Sort()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 4", "default", _baseTestStartDate.AddDays(-5), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 5", "default", _baseTestStartDate.AddDays(-6), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 6", "default", _baseTestStartDate.AddDays(-7), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 7", "default", _baseTestStartDate.AddDays(-8), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 8", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 9", "default", _baseTestStartDate.AddYears(-1), _baseTestEndDate.AddYears(1), _baseTotalSeats),
            new Event("WhatIsThis", "Not clear", _baseTestStartDate.AddMonths(-2), _baseTestEndDate.AddMonths(2), _baseTotalSeats),
            new Event("LastEvent", "last", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats),
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        var (result, totalItems) = await repository.GetPagedAsync();

        //Assert
        result.Should().NotBeNull();
        totalItems.Should().Be(testEvents.Count);
        result.First().Title.Should().Be("First event");
        result.Last().Title.Should().Be("Event 8");
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateEvent_Positive()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();
        await using var context = fixture.CreateContext();

        var eventModel = new Event(
            "Test Event",
            "Test description",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        var updatedModel = new Event(
            "Test Event Updated",
            "Test description Updated",
            new DateTime(2025, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 04, 04, 0, 0, 0, DateTimeKind.Utc),
            20);

        await context.Events.AddAsync(eventModel);
        await context.SaveChangesAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        var action = async () =>
        {
            await repository.UpdateAsync(eventModel.Id, updatedModel);
            await repository.SaveChangesAsync();
        };

        //Assert
        await action.Should().NotThrowAsync();

        await using var assertionContext = fixture.CreateContext();
        var checkEvent = await assertionContext.Events.FirstOrDefaultAsync(e => e.Title == updatedModel.Title);
        checkEvent.Should().NotBeNull();
        checkEvent.Description.Should().Be(updatedModel.Description);
    }

    [Fact]
    public async Task UpdateEvent_Negative_NotFound()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Event)} {randomGuid} is not found";

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        var action = async () => await repository.UpdateAsync(randomGuid, new Event());

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteEvent_Positive()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        await using var arrangeContext = fixture.CreateContext();
        List<Event> testEvents =
        [
            new Event("First event", "test", _baseTestStartDate, _baseTestEndDate, _baseTotalSeats),
            new Event("Holiday", "holiday", _baseTestStartDate.AddMonths(-4), _baseTestEndDate, _baseTotalSeats),
            new Event("Event 3", "default", _baseTestStartDate.AddDays(-4), _baseTestEndDate, _baseTotalSeats)
        ];
        await arrangeContext.Events.AddRangeAsync(testEvents);
        await arrangeContext.SaveChangesAsync();

        var initialCount = await arrangeContext.Events.CountAsync();
        var eventToDelete = await arrangeContext.Events.FirstAsync();

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        await repository.DeleteAsync(eventToDelete.Id);
        await repository.SaveChangesAsync();

        //Assert
        await using var assertionContext = fixture.CreateContext();
        (await assertionContext.Events.CountAsync()).Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task DeleteEvent_Negative_NotFound()
    {
        //Arrange
        await fixture.ResetDatabaseAsync();

        var randomGuid = Guid.NewGuid();
        var expectedExceptionMessage = $"{nameof(Event)} {randomGuid} is not found";

        await using var repositoryContext = fixture.CreateContext();
        var repository = new EventRepository(repositoryContext);

        //Act
        var action = () => repository.DeleteAsync(randomGuid);

        //Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage(expectedExceptionMessage);
    }

    #endregion
}
