using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using StudentUsos.Resources.LocalizedStrings;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Helpers
{
    [JsonSerializable(typeof(Dictionary<string, string>)), JsonConverter(typeof(JsonObjectToStringConverter))]
    internal partial class UtilitiesJsonContext : JsonSerializerContext
    { }

    public static class Utilities
    {
        public static bool IsAppRunningForTheFirstTime { get => CheckIfAppRunningForTheFirstTime(); }

        static bool CheckIfAppRunningForTheFirstTime()
        {
            if (LocalStorageManager.Default.TryGettingData(LocalStorageKeys.IsAppRunningForTheFirstTime, out string result))
            {
                return bool.Parse(result);
            }
            else
            {
                LocalStorageManager.Default.SetData(LocalStorageKeys.IsAppRunningForTheFirstTime, bool.TrueString);
                return true;
            }
        }

        public static float Lerp(float value, float min, float max)
        {
            return value * (max - min) + min;
        }

        public static void ShowError(string message)
        {
            ApplicationService.Default.MainThreadInvoke(() =>
            {
                MessagePopupPage.CreateAndShow(LocalizedStrings.Errors_Error, message, "ok", () => { });
            });
        }

        /// <summary>
        /// Show simple popup about exception
        /// Debug mode only: with exact line of code where the error occured
        /// </summary>
        /// <param name="ex"></param>
        public static void ShowError(Exception ex, [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            ApplicationService.Default?.MainThreadInvoke(() =>
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line = frame?.GetFileLineNumber() ?? -1;
                //Logger.Log(Logger.LogLevel.Error, "Catched error (StackTrace line = " + line + ")", ex, callerName, callerLineNumber, callerFilePath);
#if RELEASE
                //MessagePopupPage.CreateAndShow("Błąd", "CN: " + callerName + "\n M: " + ex.Message + "\n LN: " + callerLineNumber, "ok", () => { });
                //messagePopup.ShowPopup();
#endif
#if DEBUG
                //MessagePopupPage.CreateAndShow(LocalizedStrings.Errors_Error, "CN: " + callerName + "\n M: " + ex.Message + "\n FN: " + frame?.GetFileName() + "\n L: " + line, "ok", () => { });
                //messagePopup.ShowPopup();
#endif
                _ = ShowSnackBarAsync("Error " + "CN: " + callerName + "\n M: " + ex.Message + "\n LN: " + callerLineNumber, "ok");
            });
        }

        public static void ShowError(string title, string message)
        {
            MessagePopupPage.CreateAndShow(title, message, "ok", () => { });
        }

        static Color? Gray600 { get; set; }
        static Color? Primary { get; set; }
        public static async Task ShowSnackBarAsync(string text, string buttonText, Action? action = null)
        {
            if (Gray600 == null)
            {
                Gray600 = GetColorFromResources("Gray600");
            }
            if (Primary == null)
            {
                Primary = GetColorFromResources("Primary");
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Gray600,
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.White,
                CornerRadius = new CornerRadius(20),
            };

            TimeSpan duration = TimeSpan.FromSeconds(3);

            var snackbar = Snackbar.Make(text, action, buttonText, duration, snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Get color from resources by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Color GetColorFromResources(string name)
        {
            if (Application.Current == null)
            {
                return Colors.White;
            }
            var colorResource = Application.Current.Resources.MergedDictionaries.FirstOrDefault();
            if (colorResource is null)
            {
                Debug.Fail("Dictionary is null");
                return Colors.Gray;
            }
            if (colorResource[name] is Color color)
            {
                return color;
            }
            Debug.Fail("Resource is not of type Color");
            return Colors.Gray;

        }

        /// <summary>
        /// Get random color from Colors.xml with name scheme "Random" + number;
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColor()
        {
            Random random = new Random();
            List<Color> colors = new List<Color>();
            for (int i = 1; i <= 11; i++)
            {
                colors.Add(GetColorFromResources("Random" + i.ToString()));
            }
            return colors[random.Next(0, colors.Count - 1)];
        }

        public static List<T> TakeElements<T>(List<T> list, int from, int to)
        {
            List<T> result = new List<T>();
            if (from >= list.Count) return result;
            to = Math.Min(to, list.Count - 1);
            for (int i = from; i <= to; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }

        /// <summary>
        /// Lerp between colors
        /// </summary>
        /// <param name="s">starting color</param>
        /// <param name="t">end colors</param>
        /// <param name="k">lerp steep, must be between 0 and 1</param>
        /// <returns></returns>
        public static Color ColorLerp(this Color s, Color t, float k)
        {
            if (k < 0 || k > 1) throw new Exception("Parameter k has wrong value!");
            var bk = (1 - k);
            var a = s.Alpha * bk + t.Alpha * k;
            var r = s.Red * bk + t.Red * k;
            var g = s.Green * bk + t.Green * k;
            var b = s.Blue * bk + t.Blue * k;
            return new Color(r, g, b, a);
        }

        static int animationCounter = 0;
        /// <summary>
        /// Execure any animation using callback from Animation class
        /// </summary>
        /// <param name="self">Owner of animation</param>
        /// <param name="callback">Callback with parameter which takes values between 0 and 1 used to animate</param>
        /// <param name="duration">Duration of animation</param>
        /// <param name="easing">Type of easing</param>
        /// <param name="finished">Animation finished callback</param>
        public static void Animate(this VisualElement self,
            Action<double> callback,
            uint duration,
            Easing easing,
            uint rate = 16,
            Func<bool>? repeats = null,
            Action<double, bool>? finished = null)
        {
            animationCounter++;
            var animation = new Animation(callback, 0, 1, easing);
            if (repeats != null) animation.Commit(self, "Animation" + animationCounter.ToString(), rate, duration, easing, finished, repeats);
            else animation.Commit(self, "Animation" + animationCounter.ToString(), rate, duration, easing, finished);
        }

        /// <summary>
        /// Checks if given date is between date start and end
        /// </summary>
        /// <param name="date"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool CheckIfBetweenDates(DateTime date, DateTime start, DateTime end)
        {
            return DateTime.Compare(date.Date, start.Date) >= 0 && DateTime.Compare(date.Date, end.Date) <= 0;
        }

        /// <summary>
        /// Generate new DateTime with TimeOfDay set to zero
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime SetTimeToZero(DateTime dateTime)
        {
            DateTime newDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            return newDateTime;
        }

        /// <summary>
        /// Get the difference between 2 lists
        /// </summary>
        /// <typeparam name="T">Any class</typeparam>
        /// <param name="l1">First list</param>
        /// <param name="l2">Seconds list</param>
        /// <param name="comparer">Function for compraing list objects</param>
        public static void ListsDifference<T>(ref List<T> l1, ref List<T> l2, Func<T, T, bool> comparer) where T : class
        {
            for (int i = 0; i < l1.Count; i++)
            {
                for (int j = 0; j < l2.Count; j++)
                {
                    if (l1.Count == 0 || l2.Count == 0 || i < 0 || j < 0) return;
                    if (comparer(l1[i], l2[j]))
                    {
                        l1.Remove(l1[i]);
                        l2.Remove(l2[j]);
                        i--;
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// Get the difference between 2 lists
        /// </summary>
        /// <typeparam name="T">Any class</typeparam>
        /// <param name="l1">First list</param>
        /// <param name="l2">Seconds list</param>
        /// <param name="out1">Result = content of first list - content of seconds list</param>
        /// <param name="out2">Result = content of seconds list - content of first list</param>
        /// <param name="comparer">Function for compraing list objects</param>
        public static void ListsDifference<T>(IEnumerable<T> l1, IEnumerable<T> l2, out List<T> out1, out List<T> out2, Func<T, T, bool> comparer) where T : class
        {
            out1 = new List<T>(l1);
            out2 = new List<T>(l2);
            for (int i = 0; i < out1.Count; i++)
            {
                for (int j = 0; j < out2.Count; j++)
                {
                    if (out1.Count == 0 || out2.Count == 0 || j < 0 || i < 0)
                    {
                        return;
                    }
                    if (comparer(out1[i], out2[j]))
                    {
                        out1.Remove(out1[i]);
                        out2.Remove(out2[j]);
                        j = -1;
                        if (i != 0) i--;
                    }
                }
            }
        }

        /// <summary>
        /// Compare lists by it's content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <param name="comparer"></param>
        /// <returns>True if collections contain the same items, False otherwise</returns>
        public static bool CompareCollections<T>(IEnumerable<T> l1, IEnumerable<T> l2, Func<T, T, bool> comparer) where T : class
        {
            ListsDifference(l1.ToList(), l2.ToList(), out List<T> out1, out List<T> out2, comparer);
            return out1.Count == 0 && out2.Count == 0;
        }

        public static void RemoveDuplicates<T>(List<T> collection, Func<T, T, bool> comparer) where T : class
        {
            for (int i = 0; i < collection.Count; i++)
            {
                for (int j = i + 1; j < collection.Count; j++)
                {
                    if (comparer(collection[i], collection[j]))
                    {
                        collection.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// Works like .ToString() on a DateTime but with "-" as a separator and simplified by removing one of TimeOfDay/Date when they are equal in both DateTimes
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns></returns>
        public static string MergeDateTimes(DateTime firstDate, DateTime secondDate)
        {
            try
            {
                if (firstDate == secondDate)
                {
                    if (firstDate.TimeOfDay == TimeSpan.Zero) return firstDate.ToString("dd.MM.yyyy");
                    return firstDate.ToString("HH:mm dd.MM.yyyy");
                }
                if (firstDate.Date != secondDate.Date && firstDate.TimeOfDay != secondDate.TimeOfDay)
                {
                    return firstDate.ToString("HH:mm dd.MM.yyyy") + " - " + secondDate.ToString("HH:mm dd.MM.yyyy");
                }
                if (firstDate.Date != secondDate.Date)
                {
                    if (firstDate.TimeOfDay == TimeSpan.Zero) return firstDate.ToString("dd.MM.yyyy") + " - " + secondDate.ToString("dd.MM.yyyy");
                    return firstDate.ToString("HH:mm dd.MM.yyyy") + " - " + secondDate.ToString("dd.MM.yyyy");
                }
                if (firstDate.TimeOfDay != secondDate.TimeOfDay)
                {
                    return firstDate.ToString("HH:mm") + " - " + secondDate.ToString("HH:mm") + " " + firstDate.Date.ToString("dd.MM.yyyy");
                }
                return firstDate.ToString("HH:mm dd.MM.yyyy") + " - " + secondDate.ToString("HH:mm dd.MM.yyyy");
            }
            catch
            {
                return firstDate.ToString("HH:mm dd.MM.yyyy") + " - " + secondDate.ToString("HH:mm dd.MM.yyyy");
            }
        }

        public static string GetLocalizedStringFromJson(string json)
        {
            return GetLocalizedStringFromJson(json, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        }

        public static string GetLocalizedStringFromJson(string json, string languageName)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return string.Empty;
                var deserialized = JsonSerializer.Deserialize(json, UtilitiesJsonContext.Default.DictionaryStringString);
                if (deserialized is null)
                {
                    return string.Empty;
                }

                if (deserialized.TryGetValue(languageName, out var result) && string.IsNullOrEmpty(result) == false) return result;
                //first backup language
                if (deserialized.TryGetValue("en", out var result2) && string.IsNullOrEmpty(result2) == false) return result2;
                //second backup language
                if (deserialized.TryGetValue("pl", out var result3)) return result3;
                return string.Empty;
            }
            catch
            {
                return "localized string error";
            }
        }

        /// <summary>
        /// Use this when getting DateTimeOffset.Now.DateTime on app startup to make sure that 
        /// the first initialization of DateTimeOffset.Now.DateTime happens on worker thread (first usage of DateTimeOffset.Now.DateTime is expensive)
        /// </summary>
        /// <param name="result"></param>
        public static void DateTimeNowWorkerThread(Action<string> result, string toStringFormat)
        {
            ApplicationService.Default.WorkerThreadInvoke(() =>
            {
                string date = DateTimeOffset.Now.DateTime.ToString(toStringFormat);
                result?.Invoke(date);
            });
        }

    }
}
