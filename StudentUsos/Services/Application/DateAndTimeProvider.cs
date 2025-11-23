namespace StudentUsos.Services.Application;

public static class DateAndTimeProvider
{
    public static IDateAndTimeProvider Current { get; private set; } = new DefaultDateAndTimeProvider();

    public static void SwitchProvider(IDateAndTimeProvider provider)
    {
        Current = provider;
    }
}
