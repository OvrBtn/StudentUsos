namespace StudentUsos.Services.LocalStorage;

public class LocalStorageManager : ILocalStorageManager
{
    public static ILocalStorageManager Default { get; private set; }
    public LocalStorageManager()
    {
        Default = this;
    }

    public bool ContainsData(LocalStorageKeys data)
    {
        return Preferences.ContainsKey(data.ToString());
    }

    public string? GetData(LocalStorageKeys data)
    {
        var pref = Preferences.Get(data.ToString(), null);
        return pref;
    }

    public bool TryGettingData(LocalStorageKeys data, out string result)
    {
        var pref = Preferences.Get(data.ToString(), null);
        if (pref is null)
        {
            result = "";
            return false;
        }
        result = pref;
        return true;
    }

    public void SetData(LocalStorageKeys data, string value)
    {
        Preferences.Set(data.ToString(), value);
    }

    public void Remove(LocalStorageKeys data)
    {
        Preferences.Remove(data.ToString());
    }

    public void DeleteEverything()
    {
        //shouldn't be deleted, it's needed for BackwardCompatibility to work properly
        var lastCheckedVersion = Preferences.Get(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion.ToString(), null);

        Preferences.Clear();

        if (lastCheckedVersion is not null) Preferences.Set(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion.ToString(), lastCheckedVersion);
    }

}