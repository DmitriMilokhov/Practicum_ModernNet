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

                    var bookingResult = await BookStubAsync(stoppingToken);

                    using var scope = provider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    var status = bookingResult ? BookingStatus.Confirmed : BookingStatus.Rejected;
                    await bookingService.UpdateBookingStatusAsync(booking.Id, status, stoppingToken);

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

    private async Task<bool> BookStubAsync(CancellationToken ct)
    {
        try 
        {
            //here should be real Booking logic, that should return some API result which can be used instead this bool method
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            return true;
        }
        catch
        {  
            return false; 
        }
    }
}
