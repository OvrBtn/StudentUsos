
namespace StudentUsos.Services.Application;

public class DefaultDateAndTimeProvider : IDateAndTimeProvider
{
    public DateTime Now => DateTimeOffset.Now.DateTime;
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow.DateTime;
}
