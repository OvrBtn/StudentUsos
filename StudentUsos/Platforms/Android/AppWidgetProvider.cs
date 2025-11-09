using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Text;
using Java.Util;
using StudentUsos.Features.Activities.Repositories;
using Uri = Android.Net.Uri;

namespace StudentUsos.Platforms.Android
{
    [BroadcastReceiver(Label = "@string/widget_activities_schedule", Exported = true)]
    [IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE", TOGGLE_DAY_ACTION })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        public const string TOGGLE_DAY_ACTION = "StudentUsos.TOGGLE_DAY";

        public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
        {
            if (context is null || appWidgetManager is null || appWidgetIds is null)
            {
                return;
            }

            foreach (int appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId);
            }
            appWidgetManager.NotifyAppWidgetViewDataChanged(appWidgetIds, Resource.Id.listViewSchedule);
        }

        private static bool GetShowTomorrow(Context context, int widgetId)
        {
            var prefs = context.GetSharedPreferences("widgetPrefs", FileCreationMode.Private);
            return prefs?.GetBoolean($"showTomorrow_{widgetId}", false) ?? false;
        }

        private static void SetShowTomorrow(Context context, int widgetId, bool value)
        {
            var prefs = context.GetSharedPreferences("widgetPrefs", FileCreationMode.Private);
            prefs?.Edit()?.PutBoolean($"showTomorrow_{widgetId}", value)?.Apply();
        }


        public override void OnReceive(Context? context, Intent? intent)
        {
            base.OnReceive(context, intent);

            if (context is null || intent is null)
            {
                return;
            }

            if (intent.Action == TOGGLE_DAY_ACTION)
            {
                int widgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, -1);
                bool current = GetShowTomorrow(context, widgetId);
                bool next = !current;
                SetShowTomorrow(context, widgetId, next);

                var manager = AppWidgetManager.GetInstance(context);
                if (manager is not null)
                {
                    manager.NotifyAppWidgetViewDataChanged(new[] { widgetId }, Resource.Id.listViewSchedule);
                    UpdateWidget(context, manager, widgetId);
                }
            }
        }

        //avoid using DateTime.Now since loading .NET's timezone database on Android can be slow
        private static string GetCurrentFormattedDate(int addDays = 0)
        {
            var locale = Java.Util.Locale.Default;
            var formatter = DateFormat.GetDateInstance(DateFormat.Long, locale);

            var cal = Calendar.Instance;
            cal.Add(CalendarField.Date, addDays);

            return formatter.Format(cal.Time);
        }

        public static long GetUniqueId()
        {
            return SystemClock.ElapsedRealtimeNanos();
        }

        static void SetHeaderLabels(Context context, RemoteViews views, bool showTomorrow)
        {
            string todayLabel = context.GetString(Resource.String.widget_title_today);
            string tomorrowLabel = context.GetString(Resource.String.widget_title_tomorrow);
            string dateFormatted = GetCurrentFormattedDate(showTomorrow ? 1 : 0);
            views.SetTextViewText(Resource.Id.tvHeader, showTomorrow ? $"{tomorrowLabel} ({dateFormatted})" : $"{todayLabel} ({dateFormatted})");
            views.SetTextViewText(Resource.Id.btnToggleDay, showTomorrow ? todayLabel : tomorrowLabel);
        }

        static void SetStateText(Context context, RemoteViews views, bool showTomorrow)
        {
            DateTime date = AndroidHelper.GetCurrentDate().AddDays(showTomorrow ? 1 : 0);
            var activitiesRepository = App.ServiceProvider.GetService<IActivitiesRepository>()!;
            var activities = activitiesRepository!.GetActivities(date);
            if (activities is null)
            {
                views.SetViewVisibility(Resource.Id.stateView, ViewStates.Visible);
                views.SetTextViewText(Resource.Id.stateView, context.GetString(Resource.String.widget_no_data));
                views.SetViewVisibility(Resource.Id.listViewSchedule, ViewStates.Gone);
            }
            else
            {
                if (activities.AllActivities.Count > 0)
                {
                    views.SetViewVisibility(Resource.Id.stateView, ViewStates.Gone);
                    views.SetViewVisibility(Resource.Id.listViewSchedule, ViewStates.Visible);
                }
                else
                {
                    views.SetViewVisibility(Resource.Id.listViewSchedule, ViewStates.Gone);
                    views.SetViewVisibility(Resource.Id.stateView, ViewStates.Visible);
                    views.SetTextViewText(Resource.Id.stateView, context.GetString(Resource.String.widget_day_off));
                }
            }
        }

        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            bool showTomorrow = GetShowTomorrow(context, appWidgetId);

            var views = new RemoteViews(context.PackageName, Resource.Layout.widget);

            SetHeaderLabels(context, views, showTomorrow);
            SetStateText(context, views, showTomorrow);

            Intent toggleIntent = new(context, typeof(AppWidget));
            toggleIntent.SetAction(TOGGLE_DAY_ACTION);
            toggleIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
            toggleIntent.SetData(Uri.Parse($"widget://toggle/{appWidgetId}/{GetUniqueId()}"));

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
#pragma warning disable CA1416
                flags |= PendingIntentFlags.Immutable;
#pragma warning restore
            }

            PendingIntent? pendingIntent = PendingIntent.GetBroadcast(
                context,
                appWidgetId,
                toggleIntent,
                flags
            );
            views.SetOnClickPendingIntent(Resource.Id.btnToggleDay, pendingIntent);

            Intent viewsServiceIntent = new(context, typeof(ScheduleRemoteViewsService));
            viewsServiceIntent.PutExtra("showTomorrow", showTomorrow);
            viewsServiceIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
            viewsServiceIntent.SetData(Uri.Parse($"widget://service/{appWidgetId}/{GetUniqueId()}"));

            views.SetRemoteAdapter(Resource.Id.listViewSchedule, viewsServiceIntent);

            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }


    }
}
