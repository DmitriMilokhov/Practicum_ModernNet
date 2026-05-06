using EventManager.Features.Bookings.Interfaces;
using EventManager.Features.Bookings.Model;
using EventManager.Infrastructure.Interfaces;

namespace EventManager.Features.Bookings;

public class BookingBackgroundService(ILogger<BookingBackgroundService> logger,
    ITaskQueue<BookingDto> bookingQueue, IServiceProvider provider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking background service is launched");

        await foreach (var booking in bookingQueue.ReadAllAsync(stoppingToken))
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

            try
            {
                logger.LogInformation("Booking for event {eventId} has been started", booking.EventId);

                //here should be real Booking logic
                await Task.Delay(TimeSpan.FromSeconds(2), combinedCts.Token);

                await SetBookingStatus(booking.Id, BookingStatus.Confirmed, combinedCts.Token);

                logger.LogInformation("Booking for event {eventId} has been finished", booking.EventId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                await SetBookingStatus(booking.Id, BookingStatus.Rejected, combinedCts.Token);
                logger.LogWarning("Event Booking Time-out. Booking id: {id}", booking.EventId);
            }
            catch (Exception ex)
            {
                await SetBookingStatus(booking.Id, BookingStatus.Rejected, combinedCts.Token);
                logger.LogError(ex, "Error during event booking");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        logger.LogInformation("Booking background service is stopped");
    }

    private async Task SetBookingStatus(Guid bookingId, BookingStatus status, CancellationToken ct)
    {
        using var scope = provider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await bookingService.UpdateBookingStatusAsync(bookingId, status, ct);
    }
}
