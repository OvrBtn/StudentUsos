namespace StudentUsos.Services.LocalStorage;

public class LocalStorageManager : ILocalStorageManager
{
    public static ILocalStorageManager Default { get; private set; }
    public LocalStorageManager()
    {
        Default = this;
    }

    public bool ContainsData(LocalStorageKeys key)
    {
        return Preferences.ContainsKey(key.ToString());
    }

    public string? GetString(LocalStorageKeys key)
    {
        var pref = Preferences.Get(key.ToString(), null);
        return pref;
    }

    public bool TryGettingString(LocalStorageKeys key, out string value)
    {
        var pref = Preferences.Get(key.ToString(), null);
        if (pref is null)
        {
            value = "";
            return false;
        }
        value = pref;
        return true;
    }

    public void SetString(LocalStorageKeys key, string value)
    {
        Preferences.Set(key.ToString(), value);
    }

    public void Remove(LocalStorageKeys key)
    {
        Preferences.Remove(key.ToString());
    }

    public void DeleteEverything()
    {
        //shouldn't be deleted, it's needed for BackwardCompatibility to work properly
        var lastCheckedVersion = Preferences.Get(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion.ToString(), null);

        Preferences.Clear();

        if (lastCheckedVersion is not null) Preferences.Set(LocalStorageKeys.BackwardCompatibilityLastCheckedVersion.ToString(), lastCheckedVersion);
    }

}