using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure.Constants;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Events.Model;

public class EventFilterValidator : IEventFilterValidator
{
    public void Validate(EventFilter filter)
    {
        if (filter.Page < 1)
        {
            throw new ValidationException(Messages.PageMustBeAboveOrEqualOne);
        }

        if (filter.PageSize < 1)
        {
            throw new ValidationException(Messages.PageSizeMustBeAboveOrEqualOne);
        }

        if (filter.Title != null && filter.Title.All(char.IsWhiteSpace))
        {
            throw new ValidationException(Messages.TitleFilterWithoutSpacesMsg);
        }

        if (filter is { From: not null, To: not null } && filter.To <= filter.From)
        {
            throw new ValidationException(Messages.EndDateLaterThanStartMsg);
        }
    }
}
