using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Platforms;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Services.ServerConnection;
using System.Globalization;
using System.Text.Json;

namespace StudentUsos.Platforms.Android;

public class CustomNotificationBuilder : NotificationBuilder
{
    public CustomNotificationBuilder(ILogger<NotificationBuilder> logger, FirebasePushNotificationOptions options) : base(logger, options)
    {

    }

    public override bool ShouldHandleNotificationReceived(IDictionary<string, object> data)
    {
        if (data.ContainsKey("type"))
        {
            return true;
        }
        return base.ShouldHandleNotificationReceived(data);
    }

    public override async void OnNotificationReceived(IDictionary<string, object> data)
    {
        string type = string.Empty;
        if (data.TryGetValue("type", out object typeObject))
        {
            type = typeObject.ToString();
        }
        else
        {
            //if key not present then received notification is not send by internal server
            //required to support campaigns from Firebase console
            base.OnNotificationReceived(data);
            return;
        }

        if (type == "usos/grades/grade")
        {
            await HandleNewGradeNotificationAsync(data);
        }

        base.OnNotificationReceived(data);
    }

    async Task HandleNewGradeNotificationAsync(IDictionary<string, object> data)
    {
        data["title"] = data["body"] = LocalizedStrings.PushNotifications_UsosNewGrade_DefaultTitleAndBody;

        try
        {
            var examId = data["examId"].ToString();
            var examSessionNumber = data["examSessionNumber"].ToString();

            var serverConnectionManager = App.ServiceProvider.GetService<IServerConnectionManager>();

            Dictionary<string, string> args = new()
            {
                {"exam_id", examId},
                {"exam_session_number", examSessionNumber},
                {"fields", "value_symbol|course_edition[course_name]" }
            };
            var result = await serverConnectionManager.SendRequestToUsosAsync("services/grades/grade", args, timeout: 5);
            if (result == null)
            {
                return;
            }
            var deserialized = JsonSerializer.Deserialize(result, UsosNewGradePushNotificationDetailsJsonContext.Default.UsosNewGradePushNotificationDetails);
            if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.ChosenLanguageCode, out string languageCode))
            {
                CultureInfo culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                if (deserialized.CourseEdition.CourseNameLocalized.TryGetValue(languageCode, out string courseNameLocalized))
                {
                    data["body"] = courseNameLocalized;
                }
                else
                {
                    data["body"] = deserialized.CourseEdition.CourseNameLocalized.Values.FirstOrDefault(LocalizedStrings.PushNotifications_UsosNewGrade_DefaultTitleAndBody);
                }
            }
            else
            {
                string currentCultureCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                if (deserialized.CourseEdition.CourseNameLocalized.TryGetValue(currentCultureCode, out string courseNameLocalized))
                {
                    data["body"] = courseNameLocalized;
                }
                else
                {
                    data["body"] = deserialized.CourseEdition.CourseNameLocalized.Values.FirstOrDefault(LocalizedStrings.PushNotifications_UsosNewGrade_DefaultTitleAndBody);
                }
            }
            data["title"] = LocalizedStrings.PushNotifications_UsosNewGrade_InterpretedTitle;
            data["large_icon"] = $"notification_new_grade_{deserialized.ValueSymbol.Replace(".", "").ToLowerInvariant()}";
        }
        catch (Exception ex) { data["body"] += ex.Message; }
    }
}