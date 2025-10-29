using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using StudentUsos.Features.Activities.Repositories;
using Activity = StudentUsos.Features.Activities.Models.Activity;

namespace StudentUsos.Platforms.Android
{
    [Service(Permission = "android.permission.BIND_REMOTEVIEWS")]
    public class ScheduleRemoteViewsService : RemoteViewsService
    {
        public override IRemoteViewsFactory OnGetViewFactory(Intent intent)
        {
            bool showTomorrow = intent.GetBooleanExtra("showTomorrow", false);
            return new ScheduleRemoteViewsFactory(ApplicationContext, showTomorrow);
        }
    }

    public class ScheduleRemoteViewsFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
    {
        private readonly Context context;
        private readonly bool showTomorrow;
        private List<Activity> activities = new();

        public ScheduleRemoteViewsFactory(Context context, bool showTomorrow)
        {
            this.context = context;
            this.showTomorrow = showTomorrow;
        }

        public void OnCreate() { LoadData(); }

        public void OnDataSetChanged() { LoadData(); }

        public void OnDestroy() { activities.Clear(); }

        public int Count => activities.Count;

        public RemoteViews GetViewAt(int position)
        {
            Log.Debug("ScheduleWidget", "comment");

            var activity = activities[position];
            var rv = new RemoteViews(context.PackageName, Resource.Layout.widget_activity_item);

            rv.SetTextViewText(Resource.Id.tvTime, $"{activity.StartTime} - {activity.EndTime}");
            rv.SetTextViewText(Resource.Id.tvName, activity.Name);
            rv.SetTextViewText(Resource.Id.tvRoom, $"Room: {activity.RoomNumber}");
            rv.SetTextViewText(Resource.Id.tvBuilding, activity.BuildingName);

            return rv;
        }

        public RemoteViews LoadingView => null;
        public int ViewTypeCount => 1;
        public long GetItemId(int position) => position;
        public bool HasStableIds => true;

        private void LoadData()
        {
            DateTime date = DateTime.Today.AddDays(showTomorrow ? 1 : 0);
            Log.Debug("ScheduleWidget", date.ToString());
            var activitiesRepository = App.ServiceProvider.GetService<IActivitiesRepository>()!;
            activities = activitiesRepository!.GetActivities(date)?.AllActivities ?? new();
            Log.Debug("ScheduleWidget", activities.Count.ToString());
        }
    }
}
