using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Models
{
    public partial class FinalGrade : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        /// <summary>
        /// e.g. S1Inf1-ALG ; can't be used to distinguish between lecture, classes and laboratories 
        /// </summary>
        public string CourseId { get; set; }
        /// <summary>
        /// numeric value e.g. 245398 ; can be used as PrimaryKey and can be used to distinguish between lecture, classes and laboratories 
        /// </summary>
        public string CourseUnitId { get; set; }
        public string CourseName { get; set; }
        public string ClassType { get; set; }
        [Ignore]
        public Group Group { get; set; }
        public string EctsPoints { get => Group?.EctsPoints ?? "0"; }
        public float EctsPointsFloat
        {
            get
            {
                if (float.TryParse(EctsPoints, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
                {
                    return parsed;
                }
                return 0f;
            }
        }

        [JsonPropertyName("value_symbol")]
        public string ValueSymbol
        {
            get => valueSymbol;
            set
            {
                valueSymbol = value;
                OnPropertyChanged(nameof(ValueSymbol));
                OnPropertyChanged(nameof(IsSetOrModified));
                OnPropertyChanged(nameof(IsSetAndNotModified));
            }
        }
        string valueSymbol;
        [JsonPropertyName("passes")]
        public bool Passes { get; set; }
        public string ValueDescription { get => Utilities.GetLocalizedStringFromJson(ValueDescriptionJson); }
        [JsonPropertyName("value_description"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string ValueDescriptionJson { get; set; }
        [JsonPropertyName("exam_id"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string ExamId { get; set; }
        [JsonPropertyName("exam_session_number"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string ExamSessionNumber { get; set; }
        [JsonPropertyName("counts_into_average")]
        public string CountsIntoAverage { get; set; }
        [Ignore]
        public bool CountsIntoAverageBool
        {
            get
            {
                if (bool.TryParse(CountsIntoAverage, out bool parsed)) return parsed;
                if (CountsIntoAverage != null && CountsIntoAverage.ToLower().Contains('Y')) return true;
                return false;
            }
        }
        [JsonPropertyName("grade_type_id")]
        public string GradeTypeId { get; set; }
        [JsonPropertyName("date_modified")]
        public string DateModified
        {
            get => dateModified;
            set
            {
                dateModified = value;
                if (DateTime.TryParseExact(dateModified, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out DateTime parsed))
                {
                    dateModified = parsed.ToString("HH:mm:ss dd.MM.yyyy");
                }
            }

        }
        string dateModified;
        [Ignore]
        public DateTime DateModifiedDateTime
        {
            get
            {
                if (dateModifiedDateTime == DateTime.MinValue && DateTime.TryParseExact(DateModified, "HH:mm:ss dd.MM.yyyy", null, DateTimeStyles.None, out DateTime result)) dateModifiedDateTime = result;
                return dateModifiedDateTime;
            }
            set => dateModifiedDateTime = value;
        }
        DateTime dateModifiedDateTime;
        /// <summary>
        /// WARNING: USOS API can return null in "modification_author" property
        /// </summary>
        [JsonPropertyName("modification_author"), Ignore]
        public Person.Models.Person ModificationAuthor
        {
            get
            {
                if (modificationAuthor == null)
                {
                    modificationAuthor = new(ModificationAuthorId, ModificationAuthorFirstName, ModificationAuthorLastName);
                }
                return modificationAuthor;
            }
            set
            {
                modificationAuthor = value;
                if (modificationAuthor == null)
                {
                    modificationAuthor = new();
                }
                ModificationAuthorId = modificationAuthor.Id;
                ModificationAuthorFirstName = modificationAuthor.FirstName;
                ModificationAuthorLastName = modificationAuthor.LastName;
            }
        }
        Person.Models.Person? modificationAuthor = null;
        public string ModificationAuthorId { get; set; }
        public string ModificationAuthorFirstName { get; set; }
        public string ModificationAuthorLastName { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        public bool IsLatest { get; set; } = false;
        [Ignore]
        public bool IsEmpty { get => string.IsNullOrEmpty(ValueDescription) && string.IsNullOrEmpty(DateModified); }

        [Ignore]
        public bool IsModified
        {
            get => isModified;
            set
            {
                isModified = value;
                OnPropertyChanged(nameof(IsModified));
                OnPropertyChanged(nameof(IsSetOrModified));
                OnPropertyChanged(nameof(IsSetAndNotModified));
            }
        }
        bool isModified = false;

        public bool IsSetOrModified { get => isModified || !IsEmpty; }
        public bool IsSetAndNotModified { get => !isModified && !IsEmpty; }

        /// <summary>
        /// Used for storing grades distribution in runtime and local database. In case of <see cref="FinalGradeGroup"/> only 
        /// the <see cref="FinalGradeGroup.FirstTermGrade"/> will be set because API returns same data for both exam sessions
        /// </summary>
        public string GradeDistribution { get; set; }

        /// <summary>
        /// <see cref="LocalizedStrings.Modification"/>  + " " + <see cref="DateModified"/> + " " 
        /// + <see cref="ModificationAuthorFirstName"/> + " " + <see cref="ModificationAuthorLastName"/>
        /// </summary>
        public string ModificationInfo
        {
            get => LocalizedStrings.Modification + " " + DateModified + " " + ModificationAuthorFirstName + " " + ModificationAuthorLastName;
        }

        public FinalGrade()
        {

        }

        public void AssignGroup(Group group)
        {
            if (group == null) return;
            CourseId = group.CourseId;
            CourseUnitId = group.CourseUnitId;
            CourseName = group.CourseName;
            ClassType = group.ClassType;
            Group = group;
        }

        public bool IsCourseExamAndGradeEqual(FinalGrade grade)
        {
            if (grade == null) { return false; }
            return CourseUnitId == grade.CourseUnitId && CourseName == grade.CourseName && ExamId == grade.ExamId && DateModified == grade.DateModified
                && ExamSessionNumber == grade.ExamSessionNumber && ClassType == grade.ClassType;
        }

        public static bool AreCourseExamAndGradeEqual(FinalGrade g1, FinalGrade g2)
        {
            return g1.CourseUnitId == g2.CourseUnitId && g1.CourseName == g2.CourseName && g1.ExamId == g2.ExamId && g1.DateModified == g2.DateModified
                && g1.ExamSessionNumber == g2.ExamSessionNumber && g1.ClassType == g2.ClassType;
        }
    }
}
