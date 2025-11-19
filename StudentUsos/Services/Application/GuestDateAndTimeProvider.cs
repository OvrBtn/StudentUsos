
namespace StudentUsos.Services.Application;

public class GuestDateAndTimeProvider : IDateAndTimeProvider
{
    public DateTime Now => new(2025, 9, 11);
    public DateTimeOffset UtcNow => new(new(2025, 9, 11));
}
