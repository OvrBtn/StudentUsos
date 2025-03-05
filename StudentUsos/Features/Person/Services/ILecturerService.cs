using StudentUsos.Features.Person.Models;

namespace StudentUsos.Features.Person.Services;

public interface ILecturerService
{
    /// <summary>
    /// Get more detailed information about lecturers (email, titles)
    /// </summary>
    /// <param name="lecturerIds">List of lecturer ids</param>
    /// <returns>List of Lecturers or null if request failed</returns>
    public Task<List<Lecturer>?> GetDetailedLecturersAsync(List<string> lecturerIds);
    /// <summary>
    /// Same as <see cref="GetDetailedLecturersAsync(List{string})"/> but for single lecturer
    /// </summary>
    /// <param name="lecturerId"></param>
    /// <returns></returns>
    public Task<Lecturer?> GetDetailedLecturersAsync(string lecturerId);
}