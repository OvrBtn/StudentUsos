
namespace StudentUsos.Services.LocalStorage;

public class LocalStorageManager : ILocalStorageManager
{
    public static ILocalStorageManager Default { get; private set; }
    public LocalStorageManager()
    {
        Default = this;
    }

    public bool ContainsKey(LocalStorageKeys key)
    {
        return Preferences.ContainsKey(key.ToString());
    }

    public string? GetString(LocalStorageKeys key, string? defaultValue = null)
    {
        if (Preferences.ContainsKey(key.ToString()) == false)
        {
            return defaultValue;
        }
        return Preferences.Get(key.ToString(), defaultValue);
    }

    public bool TryGettingString(LocalStorageKeys key, out string value)
    {
        if (Preferences.ContainsKey(key.ToString()))
        {
            value = Preferences.Get(key.ToString(), string.Empty);
            return true;
        }
        value = string.Empty;
        return false;
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

    public int GetInt(LocalStorageKeys key, int defaultValue = default)
    {
        return Preferences.Get(key.ToString(), defaultValue);
    }

    public bool TryGettingInt(LocalStorageKeys key, out int value)
    {
        if (Preferences.ContainsKey(key.ToString()))
        {
            value = Preferences.Get(key.ToString(), default(int));
            return true;
        }
        value = default(int);
        return false;
    }

    public void SetInt(LocalStorageKeys key, int value)
    {
        Preferences.Set(key.ToString(), value);
    }

    public bool GetBool(LocalStorageKeys key, bool defaultValue = false)
    {
        return Preferences.Get(key.ToString(), defaultValue);
    }

    public bool TryGettingBool(LocalStorageKeys key, out bool value)
    {
        if (Preferences.ContainsKey(key.ToString()))
        {
            value = Preferences.Get(key.ToString(), default(bool));
            return true;
        }
        value = default(bool);
        return false;
    }

    public void SetBool(LocalStorageKeys key, bool value)
    {
        Preferences.Set(key.ToString(), value);

    }

    public DateTime GetDateTime(LocalStorageKeys key, DateTime defaultValue = default)
    {
        return Preferences.Get(key.ToString(), defaultValue);
    }

    public bool TryGettingDateTime(LocalStorageKeys key, out DateTime value)
    {
        if (Preferences.ContainsKey(key.ToString()))
        {
            value = Preferences.Get(key.ToString(), default(DateTime));
            return true;
        }
        value = default(DateTime);
        return false;
    }

    public void SetDateTime(LocalStorageKeys key, DateTime value)
    {
        Preferences.Set(key.ToString(), value);
    }
}