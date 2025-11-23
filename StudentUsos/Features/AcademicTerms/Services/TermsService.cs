#nullable enable
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.AcademicTerms.Services;

public class TermsService : ITermsService
{
    ILogger? logger;
    IServerConnectionManager serverConnectionManager;
    ITermsRepository termsRepository;
    public TermsService(IServerConnectionManager serverConnectionManager,
        ITermsRepository termsRepository,
        ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.termsRepository = termsRepository;
        this.logger = logger;
    }

    public async Task<List<Term>?> GetTermsAsync(DateTime minDate, DateTime maxDate)
    {
        var arguments = new Dictionary<string, string> {
            { "min_finish_date", minDate.ToString("yyyy-MM-dd") },
            { "max_start_date", maxDate.ToString("yyyy-MM-dd") } };
        var result = await serverConnectionManager.SendRequestToUsosAsync("services/terms/search", arguments);
        if (result is null)
        {
            return null;
        }
        var deserialized = JsonSerializer.Deserialize(result, TermJsonContext.Default.ListTerm);
        if (deserialized is null)
        {
            return null;
        }
        List<Term> terms = new();
        foreach (var term in deserialized)
        {
            if (term.Id.ToLower().Contains("l") || term.Id.ToLower().Contains("z"))
            {
                terms.Add(term);
            }
        }
        return terms;
    }

    public async Task<Term?> GetCurrentTermAsync()
    {
        try
        {
            var result = await serverConnectionManager.SendRequestToUsosAsync("services/terms/search",
                new Dictionary<string, string>
                {
                    { "min_finish_date", DateAndTimeProvider.Current.Now.ToString("yyyy-MM-dd") }
                });
            if (result is null)
            {
                return null;
            }
            var deserialized = JsonSerializer.Deserialize(result, TermJsonContext.Default.ListTerm);
            if (deserialized is null)
            {
                return null;
            }

            termsRepository.RemoveAll();
            foreach (var term in deserialized)
            {
                if (term.Id.ToLower().Contains("l") || term.Id.ToLower().Contains("z"))
                {
                    termsRepository.InsertOrReplace(term);
                    if (term.IsCurrentlyConducted)
                    {
                        return term;
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return null;
        }
    }
}