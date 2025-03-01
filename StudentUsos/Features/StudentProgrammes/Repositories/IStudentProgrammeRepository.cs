using StudentUsos.Features.StudentProgrammes.Models;

namespace StudentUsos.Features.StudentProgrammes.Repositories
{
    public interface IStudentProgrammeRepository
    {
        public List<StudentProgramme> GetAll();
        public void ClearAll();
        public void SaveAll(List<StudentProgramme> programmes);
    }
}
