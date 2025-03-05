using StudentUsos.Features.StudentProgrammes.Models;
using StudentUsos.Features.StudentProgrammes.Services.JsonModels;

namespace StudentUsos.Features.StudentProgrammes.Services;

public interface IStudentProgrammeService
{
    /// <summary>
    /// Get student's programmes
    /// </summary>
    /// <returns>returns data from local database if it isn't older than 7 days, 
    /// otherwise result of request to USOS API wil be returned, 
    /// if request to USOS API was unsuccessful then data from local database will be returned</returns>
    public Task<List<StudentProgramme>> GetStudentProgrammesAsync();

    /// <summary>
    /// Get programme which user currently participates in
    /// </summary>
    /// <returns>null if couldn't retrieve any programme, otherwise check the returns of <see cref="GetStudentProgrammesAsync"/></returns>
    public Task<StudentProgramme?> GetCurrentStudentProgrammeAsync();

    public Task<ProgrammeDetailsJson?> GetProgrammeDetailsAsync(string id);
}