using SQLite;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Payments.Models;
using StudentUsos.Features.Person.Models;
using StudentUsos.Features.StudentProgrammes.Models;
using StudentUsos.Features.UserInfo;

namespace StudentUsos.Services.LocalDatabase;

public class LocalDatabaseManager : ILocalDatabaseManager
{
    public static ILocalDatabaseManager Default { get; private set; }
    SQLiteConnection dbConnection;

    public LocalDatabaseManager(LocalDatabaseOptions option = LocalDatabaseOptions.DefaultLocalFile)
    {
        Default = this;

        if (option == LocalDatabaseOptions.InMemory)
        {
            dbConnection = new(":memory:");
        }
        else
        {
            dbConnection = new(Path.Combine(FileSystem.AppDataDirectory, "MainDB.db3"),
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            dbConnection.EnableWriteAheadLogging();
        }

        try
        {
            GenerateTables();
        }
        catch
        {
            BackwardCompatibility.ResetLocalData();
            LocalStorageManager.Default.SetData(LocalStorageKeys.IsAppRunningForTheFirstTime, true.ToString());
        }
    }

    public void ResetTables()
    {
        DeleteTables();
        GenerateTables();
    }

    public void GenerateTables()
    {
        dbConnection.CreateTable<Lecturer>();
        dbConnection.CreateTable<Activity>();
        dbConnection.CreateTable<Term>();
        dbConnection.CreateTable<Group>();
        dbConnection.CreateTable<GoogleCalendarEvent>();
        dbConnection.CreateTable<UsosCalendarEvent>();
        dbConnection.CreateTable<GoogleCalendar>();
        dbConnection.CreateTable<FinalGrade>();
        dbConnection.CreateTable<StudentProgramme>();
        dbConnection.CreateTable<TimetableDay>();
        dbConnection.CreateTable<Payment>();
        dbConnection.CreateTable<UserInfo>();
        dbConnection.CreateTable<LogRecord>();
    }

    public void DeleteTables()
    {
        LocalStorageManager.Default.SetData(LocalStorageKeys.IsAppRunningForTheFirstTime, true.ToString());
        dbConnection.DropTable<Lecturer>();
        dbConnection.DropTable<Activity>();
        dbConnection.DropTable<Term>();
        dbConnection.DropTable<Group>();
        dbConnection.DropTable<GoogleCalendarEvent>();
        dbConnection.DropTable<UsosCalendarEvent>();
        dbConnection.DropTable<GoogleCalendar>();
        dbConnection.DropTable<FinalGrade>();
        dbConnection.DropTable<StudentProgramme>();
        dbConnection.DropTable<TimetableDay>();
        dbConnection.DropTable<Payment>();
        dbConnection.DropTable<UserInfo>();
        dbConnection.DropTable<LogRecord>();
    }

    public int ClearTable<T>()
    {
        return dbConnection.Execute("DELETE FROM " + typeof(T).Name);
    }

    public int ExecuteQuery(string query)
    {
        return dbConnection.Execute(query);
    }

    public T? Get<T>(Func<T, bool> predicate) where T : new()
    {
        var result = dbConnection.Table<T>().FirstOrDefault(predicate);
        return result;
    }

    public List<T> GetAll<T>(Func<T, bool> predicate) where T : new()
    {
        var result = dbConnection.Table<T>().Where(predicate).ToList();
        return result;
    }

    public List<T> GetAll<T>() where T : new()
    {
        var result = dbConnection.Table<T>().ToList();
        if (result == null) return new List<T>();
        return result;
    }

    public void InsertOrReplace<T>(T obj)
    {
        dbConnection.InsertOrReplace(obj);
    }

    public void InsertOrReplaceAll<T>(IEnumerable<T> list) where T : new()
    {
        foreach (var item in list)
        {
            dbConnection.InsertOrReplace(item);
        }
    }

    public void Insert<T>(T obj) where T : new()
    {
        dbConnection.Insert(obj);
    }

    public void InsertAll<T>(IEnumerable<T> list) where T : new()
    {
        dbConnection.InsertAll(list);
    }

    public int Remove<T>(string whereBody)
    {
        string query = "DELETE FROM " + typeof(T).Name + " WHERE " + whereBody;
        return dbConnection.Execute(query);
    }

    public int Remove<T>(T obj) where T : class
    {
        if (obj is string)
        {
            throw new ArgumentException("Object to delete can't be a string");
        }
        return dbConnection.Delete(obj);
    }

    public bool IsTableEmpty<T>() where T : new()
    {
        var result = dbConnection.Query<T>("SELECT * FROM " + typeof(T).Name + " LIMIT 1");
        if (result == null || result.Count == 0) return true;
        return false;
    }
}