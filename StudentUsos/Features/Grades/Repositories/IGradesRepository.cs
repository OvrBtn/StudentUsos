using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Features.Grades.Repositories
{
    public interface IGradesRepository
    {
        public FinalGrade? GetLatestGrade();
        public List<FinalGrade> GetAll();
        public FinalGrade? Get(string courseUnitId);
        public void InsertOrReplace(FinalGrade finalGrade);
        public void DeleteAll();
        public void InsertAll(IEnumerable<FinalGrade> finalGrades);
    }
}
