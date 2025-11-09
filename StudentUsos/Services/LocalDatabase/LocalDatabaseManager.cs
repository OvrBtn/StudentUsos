using SQLite;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.CampusMap.Models;
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

    LocalDatabaseOptions option;
    public LocalDatabaseManager(LocalDatabaseOptions option = LocalDatabaseOptions.DefaultLocalFile)
    {
        Default = this;
        this.option = option;
    }

    bool isInitialized;
    object initializeLock = new();
    /// <summary>
    /// Initializing database is a bit heavy on startup time and using static constructor doesn't make
    /// sense due to DI but in this way it is delayed to the moment when any method is actually called
    /// </summary>
    void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        lock (initializeLock)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

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
                LocalStorageManager.Default.SetString(LocalStorageKeys.IsAppRunningForTheFirstTime, true.ToString());
            }
        }
    }

    public void ResetTables()
    {
        EnsureInitialized();

        DeleteTables();
        GenerateTables();
    }

    public void GenerateTables()
    {
        EnsureInitialized();

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
        dbConnection.CreateTable<CampusBuilding>();
        dbConnection.CreateTable<RoomInfo>();
        dbConnection.CreateTable<FloorMap>();
    }

    public void DeleteTables()
    {
        EnsureInitialized();

        LocalStorageManager.Default.SetString(LocalStorageKeys.IsAppRunningForTheFirstTime, true.ToString());
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
        dbConnection.DropTable<CampusBuilding>();
        dbConnection.DropTable<RoomInfo>();
        dbConnection.DropTable<FloorMap>();
    }

    public int ClearTable<T>()
    {
        EnsureInitialized();

        return dbConnection.Execute("DELETE FROM " + typeof(T).Name);
    }

    public int ExecuteQuery(string query)
    {
        EnsureInitialized();

        return dbConnection.Execute(query);
    }

    public T? Get<T>(Func<T, bool> predicate) where T : new()
    {
        EnsureInitialized();

        var result = dbConnection.Table<T>().FirstOrDefault(predicate);
        return result;
    }

    public List<T> GetAll<T>(Func<T, bool> predicate) where T : new()
    {
        EnsureInitialized();

        var result = dbConnection.Table<T>().Where(predicate).ToList();
        return result;
    }

    public List<T> GetAll<T>() where T : new()
    {
        EnsureInitialized();

        var result = dbConnection.Table<T>().ToList();
        if (result == null) return new List<T>();
        return result;
    }

    public void InsertOrReplace<T>(T obj)
    {
        EnsureInitialized();

        dbConnection.InsertOrReplace(obj);
    }

    public void InsertOrReplaceAll<T>(IEnumerable<T> list) where T : new()
    {
        EnsureInitialized();

        foreach (var item in list)
        {
            dbConnection.InsertOrReplace(item);
        }
    }

    public void Insert<T>(T obj) where T : new()
    {
        EnsureInitialized();

        dbConnection.Insert(obj);
    }

    public void InsertAll<T>(IEnumerable<T> list) where T : new()
    {
        EnsureInitialized();

        dbConnection.InsertAll(list);
    }

    public int Remove<T>(string whereBody)
    {
        EnsureInitialized();

        string query = "DELETE FROM " + typeof(T).Name + " WHERE " + whereBody;
        //replaced due to change to sqlite-net-e package which doesn't allow double quotation marks in queries
        query = query.Replace('"', '\'');
        return dbConnection.Execute(query);
    }

    public int Remove<T>(T obj) where T : class
    {
        EnsureInitialized();

        if (obj is string)
        {
            throw new ArgumentException("Object to delete can't be a string");
        }
        return dbConnection.Delete(obj);
    }

    public bool IsTableEmpty<T>() where T : new()
    {
        EnsureInitialized();

        var result = dbConnection.Query<T>("SELECT * FROM " + typeof(T).Name + " LIMIT 1");
        if (result == null || result.Count == 0) return true;
        return false;
    }
}