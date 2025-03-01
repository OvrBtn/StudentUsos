#nullable enable
using StudentUsos.Features.AcademicTerms.Models;
using System.Globalization;

namespace StudentUsos.Features.AcademicTerms.Repositories
{
    public class TermsRepository : ITermsRepository
    {
        ILocalDatabaseManager localDatabaseManager;
        public TermsRepository(ILocalDatabaseManager localDatabaseManager)
        {
            this.localDatabaseManager = localDatabaseManager;
        }

        public List<Term> GetActiveTerms()
        {
            var activeTerms = localDatabaseManager.GetAll<Term>(x => x.IsActive);
            return activeTerms;
        }

        public List<Term> GetAll()
        {
            return localDatabaseManager.GetAll<Term>();
        }

        public void InsertOrReplace(Term term)
        {
            localDatabaseManager.InsertOrReplace(term);
        }

        public void RemoveAll()
        {
            localDatabaseManager.ClearTable<Term>();
        }

        public bool TryGettingCurrentTerm(out Term? term)
        {
            var terms = GetAll();
            foreach (var item in terms)
            {
                bool isStartDateParsed = DateTime.TryParseExact(item.StartDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime startResult);
                bool isFinishDateParsed = DateTime.TryParseExact(item.FinishDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime finishResult);
                if (isStartDateParsed && isFinishDateParsed && Utilities.CheckIfBetweenDates(DateTimeOffset.Now.DateTime, startResult, finishResult))
                {
                    term = item;
                    return true;
                }
            }
            term = null;
            return false;
        }
    }
}
