using StudentUsos.Features.Person.Models;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Person.Services;

public class LecturerService : ILecturerService
{
    IServerConnectionManager serverConnectionManager;
    ILogger? logger;
    public LecturerService(IServerConnectionManager serverConnectionManager, ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.logger = logger;
    }

    public async Task<List<Lecturer>?> GetDetailedLecturersAsync(List<string> lecturerIds)
    {
        try
        {
            var arguemnts = new Dictionary<string, string>()
            {
                {"user_ids", string.Join("|", lecturerIds) },
                { "fields", "id|first_name|last_name|titles|has_email|email|staff_status|employment_positions|homepage_url|profile_url|phone_numbers|office_hours" }
            };
            var response = await serverConnectionManager.SendRequestToUsosAsync("services/users/users", arguemnts);
            if (response == null) return null;
            var deserialized = JsonSerializer.Deserialize(response, LecturerJsonContext.Default.DictionaryStringLecturer);
            if (deserialized is null)
            {
                return null;
            }
            List<Lecturer> lecturers = new();
            for (int i = 0; i < lecturerIds.Count; i++)
            {
                var lecturer = deserialized[lecturerIds[i]];
                lecturers.Add(lecturer);
            }
            return lecturers;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }

    public async Task<Lecturer?> GetDetailedLecturersAsync(string lecturerId)
    {
        var result = await GetDetailedLecturersAsync(new List<string>() { lecturerId });
        if (result == null || result.Count == 0) return null;
        return result[0];
    }

}