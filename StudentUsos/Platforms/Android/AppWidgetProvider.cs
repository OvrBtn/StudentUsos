using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using Uri = Android.Net.Uri;

namespace StudentUsos.Platforms.Android
{
    [BroadcastReceiver(Label = "Schedule Widget", Exported = true)]
    [IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE", TOGGLE_DAY_ACTION })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        public const string TOGGLE_DAY_ACTION = "StudentUsos.TOGGLE_DAY";

        private static bool showTomorrow = false;

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            Log.Debug("ScheduleWidget", "comment");
            foreach (int appWidgetId in appWidgetIds)
            {
                UpdateWidget(context, appWidgetManager, appWidgetId);
            }
            appWidgetManager.NotifyAppWidgetViewDataChanged(appWidgetIds, Resource.Id.listViewSchedule);
        }

        private static bool GetShowTomorrow(Context context, int widgetId)
        {
            var prefs = context.GetSharedPreferences("widgetPrefs", FileCreationMode.Private);
            return prefs.GetBoolean($"showTomorrow_{widgetId}", false);
        }

        private static void SetShowTomorrow(Context context, int widgetId, bool value)
        {
            var prefs = context.GetSharedPreferences("widgetPrefs", FileCreationMode.Private);
            prefs.Edit().PutBoolean($"showTomorrow_{widgetId}", value).Apply();
        }


        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            if (intent.Action == TOGGLE_DAY_ACTION)
            {
                int widgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, -1);
                bool current = GetShowTomorrow(context, widgetId);
                bool next = !current;
                SetShowTomorrow(context, widgetId, next);

                AppWidgetManager manager = AppWidgetManager.GetInstance(context);
                UpdateWidget(context, manager, widgetId);
                manager.NotifyAppWidgetViewDataChanged(new[] { widgetId }, Resource.Id.listViewSchedule);
            }
        }

        private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
        {
            bool showTomorrow = GetShowTomorrow(context, appWidgetId);

            var views = new RemoteViews(context.PackageName, Resource.Layout.widget);

            views.SetTextViewText(Resource.Id.tvHeader, showTomorrow ? "Tomorrow's Schedule" : "Today's Schedule");
            views.SetTextViewText(Resource.Id.btnToggleDay, showTomorrow ? "Today" : "Tomorrow");

            Intent toggleIntent = new(context, typeof(AppWidget));
            toggleIntent.SetAction(TOGGLE_DAY_ACTION);
            toggleIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);
            toggleIntent.SetData(Uri.Parse($"widget://toggle/{appWidgetId}/{System.DateTime.Now.Ticks}"));

            PendingIntent pendingIntent = PendingIntent.GetBroadcast(
                context,
                appWidgetId,
                toggleIntent,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
            );

            views.SetOnClickPendingIntent(Resource.Id.btnToggleDay, pendingIntent);

            Intent svcIntent = new(context, typeof(ScheduleRemoteViewsService));
            svcIntent.PutExtra("showTomorrow", showTomorrow);
            svcIntent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);

            svcIntent.SetData(Uri.Parse($"widget://service/{appWidgetId}/{System.DateTime.Now.Ticks}"));

            views.SetRemoteAdapter(Resource.Id.listViewSchedule, svcIntent);

            appWidgetManager.UpdateAppWidget(appWidgetId, views);
        }


    }
}
