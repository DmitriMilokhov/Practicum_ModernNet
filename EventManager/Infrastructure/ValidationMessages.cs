namespace EventManager.Infrastructure;

public static class ValidationMessages
{
    public const string TitleIsRequiredMsg = "Title is required";
    public const string StartAtIsRequiredMsg = "StartAt is required";
    public const string EndAtIsRequiredMsg = "EndAt is required";
    public const string EndDateLaterThanStartMsg = "End date/time must be later than start one";
    public const string PageMustBeAboveOrEqualOne = "Page must be greater than or equal to 1";
    public const string PageSizeMustBeAboveOrEqualOne = "PageSize must be greater than or equal to 1";
    public const string TitleFilterWithoutSpacesMsg = "Title filter should not contain only white space";
}
