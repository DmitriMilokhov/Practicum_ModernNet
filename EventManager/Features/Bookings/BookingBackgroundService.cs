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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking background service is launched");

        //FOR_REVIEWER: 
        //1. Осознанно убрал отсюда глобальный семафор (в последнем коммите), т.к. сделал синхронизацию по событию
        //(см. CreateBookingAsync, RejectBookingAndReleaseEvent в сервсие)
        //В противном случае синхронизация по событию невилируется этим глобальным семафором.
        //2. Использую Parallel.ForEachAsync вместо Task.WhenAll так как хочу оставить очередь на основе Channel
        await Parallel.ForEachAsync(
            bookingQueue.ReadAllAsync(stoppingToken),
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = stoppingToken },
            async (booking, ct) =>
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            using var scope = scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            try
            {
                await ProcessBookingAsync(bookingService, booking, combinedCts.Token);
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
                await bookingService.RejectBookingAndReleaseEvent(booking.Id, ct);
                logger.LogError(ex, "Error during event booking. BookingId: {bookingId}, EventId: {eventId}", 
                    booking.Id, booking.EventId);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        });

        logger.LogInformation("Booking background service is stopped");
    }

    private async Task ProcessBookingAsync(IBookingService bookingService, BookingDto booking, CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking {bookingId} for event {eventId} has been started", booking.Id, booking.EventId);

        //here should be real Booking logic
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        var isEventExist = await eventRepository.ExistsAsync(booking.EventId, stoppingToken);
        if(!isEventExist)
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
