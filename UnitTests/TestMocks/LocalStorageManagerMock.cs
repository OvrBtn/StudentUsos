using StudentUsos.Services.LocalStorage;

namespace UnitTests.TestMocks;

public class LocalStorageManagerMock : ILocalStorageManager
{
    Dictionary<string, string> dictionaryString = new();
    Dictionary<string, int> dictionaryInt = new();
    Dictionary<string, bool> dictionaryBool = new();
    Dictionary<string, DateTime> dictionaryDateTime = new();
    public bool ContainsKey(LocalStorageKeys data)
    {
        return dictionaryString.ContainsKey(data.ToString());
    }

    public void DeleteEverything()
    {
        dictionaryString = new();
    }

    public bool GetBool(LocalStorageKeys key, bool defaultValue)
    {
        if (dictionaryBool.ContainsKey(key.ToString()))
        {
            return dictionaryBool[key.ToString()];
        }
        return defaultValue;
    }

    public DateTime GetDateTime(LocalStorageKeys key, DateTime defaultValue)
    {
        if (dictionaryBool.ContainsKey(key.ToString()))
        {
            return dictionaryDateTime[key.ToString()];
        }
        return defaultValue;
    }

    public int GetInt(LocalStorageKeys key, int defaultValue)
    {
        if (dictionaryBool.ContainsKey(key.ToString()))
        {
            return dictionaryInt[key.ToString()];
        }
        return defaultValue;
    }

    public string GetString(LocalStorageKeys data)
    {
        return dictionaryString[data.ToString()];
    }

    public string? GetString(LocalStorageKeys key, string? defaultValue)
    {
        throw new NotImplementedException();
    }

    public void Remove(LocalStorageKeys data)
    {
        dictionaryString.Remove(data.ToString());
    }

    public void SetBool(LocalStorageKeys key, bool value)
    {
        dictionaryBool[key.ToString()] = value;
    }

    public void SetDateTime(LocalStorageKeys key, DateTime value)
    {
        dictionaryDateTime[key.ToString()] = value;
    }

    public void SetInt(LocalStorageKeys key, int value)
    {
        dictionaryInt[key.ToString()] = value;
    }

    public void SetString(LocalStorageKeys data, string value)
    {
        dictionaryString[data.ToString()] = value;
    }

    public bool TryGettingBool(LocalStorageKeys key, out bool value)
    {
        if (dictionaryBool.ContainsKey(key.ToString()))
        {
            value = dictionaryBool[key.ToString()];
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGettingDateTime(LocalStorageKeys key, out DateTime value)
    {
        if (dictionaryDateTime.ContainsKey(key.ToString()))
        {
            value = dictionaryDateTime[key.ToString()];
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGettingInt(LocalStorageKeys key, out int value)
    {
        if (dictionaryInt.ContainsKey(key.ToString()))
        {
            value = dictionaryInt[key.ToString()];
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGettingString(LocalStorageKeys key, out string value)
    {
        if (dictionaryString.ContainsKey(key.ToString()))
        {
            value = dictionaryString[key.ToString()];
            return true;
        }
        value = string.Empty;
        return false;
    }
}