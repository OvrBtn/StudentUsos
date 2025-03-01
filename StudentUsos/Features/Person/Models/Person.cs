using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Person.Models
{
    [JsonSerializable(typeof(List<Person>))]
    internal partial class PersonJsonContext : JsonSerializerContext
    { }

    public partial class Person : ObservableObject
    {
        [PrimaryKey, JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        [JsonPropertyName("has_email"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string HasEmail { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [Ignore]
        public string Initials { get => string.Join("", FirstName[0], LastName[0]); }
        [Ignore]
        public Color InitialsBackgroundColor
        {
            get
            {
                if (initialsBackgroundColor == null)
                {
                    initialsBackgroundColor = Utilities.GetRandomColor();
                }
                return initialsBackgroundColor;
            }
            set
            {
                initialsBackgroundColor = value;
            }
        }
        Color? initialsBackgroundColor = null;
        [Ignore]
        public string FullName { get => FirstName + " " + LastName; }


        public Person(string id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public Person()
        {

        }
    }
}
