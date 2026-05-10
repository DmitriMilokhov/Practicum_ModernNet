using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure.Interfaces;

namespace EventManager.Features.Bookings;

public class BookingBackgroundService(ILogger<BookingBackgroundService> logger,
    ITaskQueue<BookingDto> bookingQueue,
    IServiceScopeFactory scopeFactory,
    IEventRepository eventRepository) : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly SemaphoreSlim _rejectedSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking background service is launched");

        //COMMENT FOR REVIEWER: использую Parallel.ForEachAsync вместо Task.WhenAll, так как хочу оставить очередь на основе Channel
        await Parallel.ForEachAsync(
            bookingQueue.ReadAllAsync(stoppingToken),
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = stoppingToken },
            async (booking, ct) =>
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            try
            {
                await ProcessBookingAsync(booking, combinedCts.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                await SetBookingRejected(booking.Id, booking.EventId, ct);
                logger.LogWarning("Event Booking Time-out. Booking id: {id}", booking.Id);
            }
            catch (Exception ex)
            {
                await SetBookingRejected(booking.Id, booking.EventId, combinedCts.Token);
                logger.LogError(ex, "Error during event booking");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        });

        logger.LogInformation("Booking background service is stopped");
    }

    private async Task ProcessBookingAsync(BookingDto booking, CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking for event {eventId} has been started", booking.EventId);

        //here should be real Booking logic
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        await _processingSemaphore.WaitAsync(stoppingToken);
        try
        {
            var isEventExist = await eventRepository.ExistsAsync(booking.EventId, stoppingToken);
            if(!isEventExist)
            {
                await SetBookingStatus(booking.Id, BookingStatus.Rejected, stoppingToken);
                logger.LogWarning("Event Booking Rejected. Event not found. EventId:{eventId}, BookingId: {bookingId}",
                    booking.EventId, booking.Id);

                return;
            }

            await SetBookingStatus(booking.Id, BookingStatus.Confirmed, stoppingToken);
        }
        finally
        {
            _processingSemaphore.Release();
        }

        logger.LogInformation("Booking for event {eventId} has been finished", booking.EventId);
    }


    private async Task SetBookingRejected(Guid bookingId, Guid eventId, CancellationToken ct)
    {
        await _rejectedSemaphore.WaitAsync(ct);
        try 
        {
            var eventToUpdate = await eventRepository.GetAsync(eventId, ct);
            eventToUpdate.ReleaseSeats();
            await SetBookingStatus(bookingId, BookingStatus.Rejected, ct);
        }
        finally
        {
            _rejectedSemaphore.Release();
        }
    }

    private async Task SetBookingStatus(Guid bookingId, BookingStatus status, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await bookingService.UpdateBookingStatusAsync(bookingId, status, ct);
    }

    
}
