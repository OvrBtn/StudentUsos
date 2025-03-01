#nullable enable
using StudentUsos.Features.AcademicTerms.Models;

namespace StudentUsos.Features.AcademicTerms.Repositories
{
    public interface ITermsRepository
    {
        public void RemoveAll();
        public void InsertOrReplace(Term term);
        public List<Term> GetActiveTerms();
        public List<Term> GetAll();
        /// <summary>
        /// Try getting current term from the local database
        /// </summary>
        /// <returns>False if current term doesn't exist in local database, true otherwise</returns>
        public bool TryGettingCurrentTerm(out Term? term);
    }
}
