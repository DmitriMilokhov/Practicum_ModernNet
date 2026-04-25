using EventManager.Features.Events.Interfaces;
using EventManager.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Events.Model;

public class EventFilterValidator : IEventFilterValidator
{
    public void Validate(EventFilter filter)
    {
        if (filter.Page < 1)
        {
            throw new ValidationException(ValidationMessages.PageMustBeAboveOrEqualOne);
        }

        if (filter.PageSize < 1)
        {
            throw new ValidationException(ValidationMessages.PageSizeMustBeAboveOrEqualOne);
        }

        if (filter.Title != null && filter.Title.All(char.IsWhiteSpace))
        {
            throw new ValidationException(ValidationMessages.TitleFilterWithoutSpacesMsg);
        }

        if (filter is { From: not null, To: not null } && filter.To <= filter.From)
        {
            throw new ValidationException(ValidationMessages.EndDateLaterThanStartMsg);
        }
    }
}
