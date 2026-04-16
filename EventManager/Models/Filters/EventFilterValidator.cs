using EventManager.Infrastructure;
using EventManager.Interfaces.IFilters;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Filters;

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

        if (filter.Title is " ")
        {
            throw new ValidationException(ValidationMessages.TitleFilterWithoutSpacesMsg);
        }

        if (filter is { From: not null, To: not null } && filter.To <= filter.From)
        {
            throw new ValidationException(ValidationMessages.EndDateLaterThanStartMsg);
        }
    }
}
