using EventManager.Features.Bookings.Model;
using System.Diagnostics.CodeAnalysis;

namespace EventManager.Infrastructure.Interfaces;

public interface ITaskQueue<T>
{
    void Enqueue(T bookingDto);
    bool TryDequeue([MaybeNullWhen(false)] out T bookingDto);
}
