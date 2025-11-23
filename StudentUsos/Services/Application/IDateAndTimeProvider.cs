namespace StudentUsos.Services.Application;

public interface IDateAndTimeProvider
{
    public DateTime Now { get; }
    public DateTimeOffset UtcNow { get; }
}
