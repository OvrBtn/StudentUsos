using StudentUsos.Features.Person.Models;

namespace StudentUsos.Features.Person.Repositories;

public class LecturerRepository : ILecturerRepository
{
    ILocalDatabaseManager localDatabaseManager;
    public LecturerRepository(ILocalDatabaseManager localDatabaseManager)
    {
        this.localDatabaseManager = localDatabaseManager;
    }

    public void InsertOrReplaceAll(IEnumerable<Lecturer> lecturers)
    {
        localDatabaseManager.InsertOrReplaceAll(lecturers);
    }

    public void InsertOrReplace(Lecturer lecturer)
    {
        localDatabaseManager.InsertOrReplace(lecturer);
    }

    public List<Lecturer> Get(IEnumerable<string> ids)
    {
        return localDatabaseManager.GetAll<Lecturer>(x => ids.Contains(x.Id));
    }

    public Lecturer? Get(string id)
    {
        return localDatabaseManager.Get<Lecturer>(x => x.Id == id);
    }
}