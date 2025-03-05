#nullable enable
using StudentUsos.Features.AcademicTerms.Models;

namespace StudentUsos.Features.AcademicTerms.Services;

public interface ITermsService
{
    /// <summary>
    /// Get terms which are finished on minDate or later and start on maxDate or earlier
    /// </summary>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <returns></returns>
    public Task<List<Term>?> GetTermsAsync(DateTime minDate, DateTime maxDate);

    public Task<Term?> GetCurrentTermAsync();
}