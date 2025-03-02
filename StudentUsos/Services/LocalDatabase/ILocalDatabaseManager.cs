namespace StudentUsos.Services.LocalDatabase
{
    public interface ILocalDatabaseManager
    {
        public void ResetTables();
        public void GenerateTables();
        public void DeleteTables();
        public int ClearTable<T>();
        public int ExecuteQuery(string query);
        public T? Get<T>(Func<T, bool> predicate) where T : new();
        public List<T> GetAll<T>(Func<T, bool> predicate) where T : new();
        public List<T> GetAll<T>() where T : new();
        public void InsertOrReplace<T>(T obj);
        public void InsertOrReplaceAll<T>(IEnumerable<T> list) where T : new();
        public void Insert<T>(T obj) where T : new();
        public void InsertAll<T>(IEnumerable<T> list) where T : new();
        public int Remove<T>(string whereBody);
        public int Remove<T>(T obj) where T : class;
        public bool IsTableEmpty<T>() where T : new();
    }
}
