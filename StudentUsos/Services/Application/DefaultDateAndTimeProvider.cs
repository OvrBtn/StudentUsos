
namespace StudentUsos.Services.Application;

public class DefaultDateAndTimeProvider : IDateAndTimeProvider
{
    public DateTime DateTimeNow => DateTimeOffset.Now.DateTime;
}
