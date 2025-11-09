namespace StudentUsos.Services;

public interface IBackgroundJobService
{
    public void InitializeJobs();

    public void SetActivitiesBackgroundSynchronizationEnabled(bool isEnabled);
}
