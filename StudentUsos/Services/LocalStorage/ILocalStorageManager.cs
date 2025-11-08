namespace StudentUsos.Services.LocalStorage;

public interface ILocalStorageManager
{
    public bool ContainsData(LocalStorageKeys key);
    public void Remove(LocalStorageKeys key);
    public void DeleteEverything();

    public string? GetString(LocalStorageKeys key, string? defaultValue);
    public bool TryGettingString(LocalStorageKeys key, out string value);
    public void SetString(LocalStorageKeys key, string value);

    public int GetInt(LocalStorageKeys key, int defaultValue);
    public bool TryGettingInt(LocalStorageKeys key, out int value);
    public void SetInt(LocalStorageKeys key, int value);

    public bool GetBool(LocalStorageKeys key, bool defaultValue);
    public bool TryGettingBool(LocalStorageKeys key, out bool value);
    public void SetBool(LocalStorageKeys key, bool value);

    public DateTime GetDateTime(LocalStorageKeys key, DateTime defaultValue);
    public bool TryGettingDateTime(LocalStorageKeys key, out DateTime value);
    public void SetDateTime(LocalStorageKeys key, DateTime value);

}