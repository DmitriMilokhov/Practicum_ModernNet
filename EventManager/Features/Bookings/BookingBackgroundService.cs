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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (bookingQueue.TryDequeue(out var booking))
                {
                    logger.LogInformation("Booking for event {eventId} has been started", booking.EventId);

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                    using var scope = provider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    await bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed, stoppingToken);

                    logger.LogInformation("Booking for event {eventId} has been finished", booking.EventId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "Error during event booking");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        logger.LogInformation("Booking background service is stopped");
    }
}
