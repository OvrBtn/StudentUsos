using SQLite;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.AcademicTerms.Models
{
    [JsonSerializable(typeof(List<Term>))]
    internal partial class TermJsonContext : JsonSerializerContext
    { }

    public class Term
    {
        [PrimaryKey, JsonPropertyName("id")]
        public string Id { get; set; }
        public string Name { get => Utilities.GetLocalizedStringFromJson(NameJson); }
        [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string NameJson { get; set; }
        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }
        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }
        [JsonPropertyName("order_key"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string OrderKey { get; set; }
        [JsonPropertyName("finish_date")]
        public string FinishDate { get; set; }
        public bool IsCurrentlyConducted { get => CheckIsCurrentlyConducted(); }
        /// <summary>
        /// WARNING: USOS is made in a way that multiple terms can be active at the same time and terms can be active even after the finish_date
        /// </summary>
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        public Term()
        {

        }

        bool CheckIsCurrentlyConducted()
        {
            if (DateTime.TryParseExact(StartDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime startDateTime) &&
             DateTime.TryParseExact(FinishDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime finishDateTime))
            {
                if (Utilities.CheckIfBetweenDates(DateTimeOffset.Now.DateTime, startDateTime, finishDateTime)) return true;
                return false;
            }
            return false;
        }
    }
}
