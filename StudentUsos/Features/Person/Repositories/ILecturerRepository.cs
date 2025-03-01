using StudentUsos.Features.Person.Models;

namespace StudentUsos.Features.Person.Repositories
{
    public interface ILecturerRepository
    {
        public void InsertOrReplaceAll(IEnumerable<Lecturer> lecturers);
        public void InsertOrReplace(Lecturer lecturer);
        public List<Lecturer> Get(IEnumerable<string> ids);
        public Lecturer? Get(string id);
    }
}
