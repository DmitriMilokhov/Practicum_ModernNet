using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Features.Events.Interfaces;
using EventManager.Features.Events.Model;
using EventManager.Infrastructure.Exceptions;
using EventManager.Infrastructure.Interfaces;

namespace EventManager.Features.Bookings;

public class BookingBackgroundService(ILogger<BookingBackgroundService> logger,
    ITaskQueue<BookingDto> bookingQueue,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const int BookingProcessingTimeoutSec = 10;
    private const int ParallelismDegree = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking background service is launched");

        await Parallel.ForEachAsync(
            bookingQueue.ReadAllAsync(stoppingToken),
            new ParallelOptions { MaxDegreeOfParallelism = ParallelismDegree, CancellationToken = stoppingToken },
            async (booking, ct) =>
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(BookingProcessingTimeoutSec));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            await using var scope = scopeFactory.CreateAsyncScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            try
            {
                await ProcessBookingAsync(bookingService, eventRepository, booking, combinedCts.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                await bookingService.RejectBookingAndReleaseEvent(booking.Id, ct);
                logger.LogWarning("Event Booking Time-out. BookingId: {bookingId}, EventId: {eventId}", 
                    booking.Id, booking.EventId);
            }
            catch (Exception ex)
            {
                await bookingService.RejectBookingAndReleaseEvent(booking.Id, CancellationToken.None);
                logger.LogError(ex, "Error during event booking. BookingId: {bookingId}, EventId: {eventId}", 
                    booking.Id, booking.EventId);
            }
        });

        logger.LogInformation("Booking background service is stopped");
    }

    private async Task ProcessBookingAsync(IBookingService bookingService, 
        IEventRepository eventRepository,
        BookingDto booking, 
        CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking {bookingId} for event {eventId} has been started", booking.Id, booking.EventId);

        //here should be real Booking logic
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        try
        {
            await eventRepository.GetAsync(booking.EventId, stoppingToken);
        }
        catch (EntityNotFoundException ex) when (ex.EntityName == nameof(Event))
        {
            await bookingService.RejectBooking(booking.Id, stoppingToken);
            logger.LogWarning("Event Booking Rejected. Event not found. BookingId: {bookingId}, EventId:{eventId},",
                booking.Id, booking.EventId);

            return;
        }

        await bookingService.ConfirmBooking(booking.Id, stoppingToken);

        logger.LogInformation("Booking {bookingId} for event {eventId} has been successfully finished", booking.Id, booking.EventId);
    }
}
