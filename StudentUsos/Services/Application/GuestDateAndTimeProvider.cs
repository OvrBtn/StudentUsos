
namespace StudentUsos.Services.Application;

public class GuestDateAndTimeProvider : IDateAndTimeProvider
{
    public DateTime DateTimeNow => new(2025, 11, 11);
}
