using StudentUsos.Features.StudentProgrammes.Models;

namespace StudentUsos.Features.StudentProgrammes.Repositories;

public class StudentProgrammeRepository : IStudentProgrammeRepository
{
    ILocalDatabaseManager localDatabaseManager;
    public StudentProgrammeRepository(ILocalDatabaseManager localDatabaseManager)
    {
        this.localDatabaseManager = localDatabaseManager;
    }

    public List<StudentProgramme> GetAll()
    {
        return localDatabaseManager.GetAll<StudentProgramme>();
    }

    public void ClearAll()
    {
        localDatabaseManager.ClearTable<StudentProgramme>();
    }

    public void SaveAll(List<StudentProgramme> programmes)
    {
        localDatabaseManager.InsertAll(programmes);
    }
}