using StudentUsos.Features.StudentProgrammes.Models;
using StudentUsos.Features.StudentProgrammes.Repositories;
using StudentUsos.Features.StudentProgrammes.Services.JsonModels;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.StudentProgrammes.Services;

[JsonSerializable(typeof(ProgrammeDetailsJson)), JsonSerializable(typeof(List<StudentProgrammeJson>))]
internal partial class StudentProgrammeJsonContext : JsonSerializerContext
{ }

public class StudentProgrammeService : IStudentProgrammeService
{
    IServerConnectionManager serverConnectionManager;
    IStudentProgrammeRepository studentProgrammeRepository;
    ILogger? logger;
    public StudentProgrammeService(IServerConnectionManager serverConnectionManager,
        IStudentProgrammeRepository studentProgrammeRepository,
        ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.studentProgrammeRepository = studentProgrammeRepository;
        this.logger = logger;
    }

    public async Task<List<StudentProgramme>> GetStudentProgrammesAsync()
    {
        try
        {
            //return data from local database if it isn't older than 7 days
            TimeSpan refreshInterval = TimeSpan.FromDays(7);
            var localData = studentProgrammeRepository.GetAll();
            if (localData.Count > 0 && DateTime.TryParse(localData[0].CreationDate, out DateTime parsed) && DateAndTimeProvider.Current.Now - parsed < refreshInterval)
            {
                return localData;
            }

            //get student's programmes from USOS API
            var programmesResult = await GetBasicStudentProgrammeAsync();
            if (programmesResult == null) return localData;
            studentProgrammeRepository.ClearAll();
            foreach (var studentProgramme in programmesResult)
            {
                var programmeDetailsResponse = await GetProgrammeDetailsAsync(studentProgramme.ProgrammeId);
                if (programmeDetailsResponse != null)
                {
                    studentProgramme.AssignProgrammeDetailsJson(programmeDetailsResponse);
                }
            }

            studentProgrammeRepository.SaveAll(programmesResult);

            return programmesResult;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return studentProgrammeRepository.GetAll();
        }
    }

    async Task<List<StudentProgramme>?> GetBasicStudentProgrammeAsync()
    {
        var studentRequestArguments = new Dictionary<string, string>() { { "fields", "id|user|programme|status|admission_date|stages|is_primary" } };
        var studentResponse = await serverConnectionManager.SendRequestToUsosAsync("services/progs/student", studentRequestArguments);
        if (studentResponse == null)
        {
            return null;
        }
        var deserialized = JsonSerializer.Deserialize(studentResponse, StudentProgrammeJsonContext.Default.ListStudentProgrammeJson);
        if (deserialized is null)
        {
            return null;
        }
        List<StudentProgramme> studentProgrammes = new();
        foreach (var item in deserialized)
        {
            StudentProgramme programme = item.ToStudentProgramme();
            programme.CreationDate = DateAndTimeProvider.Current.Now.ToString();
            studentProgrammes.Add(programme);
        }
        return studentProgrammes;
    }

    public async Task<ProgrammeDetailsJson?> GetProgrammeDetailsAsync(string id)
    {
        var arguments = new Dictionary<string, string>()
        {
            {"programme_id", id },
            {"fields", "id|name|faculty[id|name]|all_faculties|mode_of_studies|level_of_studies|duration|professional_status|level" }
        };
        var response = await serverConnectionManager.SendRequestToUsosAsync("services/progs/programme", arguments);
        if (response == null) return null;
        var deserialized = JsonSerializer.Deserialize(response, StudentProgrammeJsonContext.Default.ProgrammeDetailsJson);
        return deserialized;
    }

    public async Task<StudentProgramme?> GetCurrentStudentProgrammeAsync()
    {
        var allStudentProgrammes = await GetStudentProgrammesAsync();
        if (allStudentProgrammes.Count == 0)
        {
            return null;
        }
        //I assume that if user had more than one student programme then the current one (the latest) will be the last programme
        return allStudentProgrammes[allStudentProgrammes.Count - 1];
    }
}